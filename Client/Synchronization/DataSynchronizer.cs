using System.Net.Sockets;
using System.Text.Json;
using BlockchainLib.Validator;
using Client.Synchronization;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace Client;

public class DataSynchronizer
{
    private RequestPool _requestPool;
    private RequestFactory _requestFactory;
    private RequestExecutor _requestExecutor;
    private IDbProcessor _dbProcessor;

    public DataSynchronizer(IDbProcessor dbProcessor, AppDbContext appDbContext)
    {
        _requestPool = new RequestPool();
        _requestFactory = new RequestFactory(dbProcessor, appDbContext);
        _requestExecutor = new RequestExecutor();
        _dbProcessor = dbProcessor;
    }

    public async Task StartSynchronization(TcpClient tcpClient, IModel peer)
    {
        try
        {
            if (tcpClient == null || peer == null)
            {
                Logger.Log("TcpClient or Peer is null", LogLevel.Error, Source.Client);
                return;
            }
            Logger.Log("Starting synchronization...", LogLevel.Information, Source.Client);
            await _requestFactory.CreateKeyExchageRequest(_requestPool, peer);

            var checkDataTask = CheckDataLoop(tcpClient, peer);
            await checkDataTask;
        }
        catch (Exception e)
        {
            Logger.Log($"Error during synchronization: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }
    
    private async Task CheckDataLoop(TcpClient tcpClient, IModel peer)
    {
        while (tcpClient.Connected)
        {
            if (tcpClient == null || peer == null)
            {
                Logger.Log("TcpClient or Peer is null", LogLevel.Error, Source.Client);
                return;
            }
            await CheckData(tcpClient, peer);
            Logger.Log("Request execution finished.", LogLevel.Information, Source.Client);
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }

    private async Task CheckData(TcpClient tcpClient, IModel peer)
    {
        try
        {
            if (tcpClient == null || peer == null)
            {
                Logger.Log("TcpClient or Peer is null", LogLevel.Error, Source.Client);
                return;
            }
            Logger.Log("Starting data check...", LogLevel.Information, Source.Client);

            var foreignData = await GetForeignData(tcpClient, peer);
            var myData = await GetMyData(tcpClient, peer);

            LogDataComparison(foreignData, myData);

            if (foreignData.TransactionIds.Count <= myData.BlocksCount)
            {
                var missingTransactions = myData.TransactionIds
                    .Except(foreignData.TransactionIds)
                    .ToList();

                if (missingTransactions.Any())
                {
                    Logger.Log($"Found missing transactions: {string.Join(", ", missingTransactions)}", LogLevel.Information, Source.Client);
                    await _requestFactory.CreateTransactionRequest(_requestPool, peer, missingTransactions);
                }
                else
                {
                    Logger.Log("No missing transactions found.", LogLevel.Information, Source.Client);
                }
            }

            if (foreignData.LastBlockHash != myData.LastBlockHash)
            {
                Logger.Log("Block hash mismatch detected, validating blockchain...", LogLevel.Information, Source.Client);
                await ValidateAndRequestBlocks(peer);
            }

            Logger.Log("Data check completed.", LogLevel.Information, Source.Client);
            await _requestExecutor.StartExecution(tcpClient, _requestPool);
        }
        catch (Exception e)
        {
            Logger.Log($"Error during data check: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

    private void LogDataComparison(InfoSyncRequest foreignData, InfoSyncRequest myData)
    {
        if (foreignData.BlocksCount < myData.BlocksCount)
        {
            Logger.Log($"The connected node has fewer blocks than the local node: foreign node - {foreignData.BlocksCount}, this node - {myData.BlocksCount}", LogLevel.Information, Source.Client);
        }
        else if (foreignData.BlocksCount == myData.BlocksCount)
        {
            Logger.Log($"Both nodes have the same number of blocks: foreign node - {foreignData.BlocksCount}, this node - {myData.BlocksCount}", LogLevel.Information, Source.Client);
        }
        else
        {
            Logger.Log($"The connected node has more blocks than the local node: foreign node - {foreignData.BlocksCount}, this node - {myData.BlocksCount}", LogLevel.Information, Source.Client);
        }
    }

    private async Task<InfoSyncRequest> GetForeignData(TcpClient tcpClient, IModel peer)
    {
        try
        {
            var knownPeer = peer as PeerInfoModel;
            var terProtocol = new TerProtocol<object>(new TerHeader(TerMessageType.InfoSync, knownPeer.NodeId, MethodType.Get), new TerPayload<object>());
            var requestHandler = new RequestHandler();
            string requestEnc = await _requestExecutor.EncryptRequest(terProtocol);
            terProtocol.Payload.Data = requestEnc;
            string json = terProtocol.Serialize();
            await requestHandler.SendRequestAsync(tcpClient, json, tcpClient.GetStream());

            var responseHandler = new ResponseHandler();
            var response = await responseHandler.ReceiveResponse(tcpClient);

            return JsonSerializer.Deserialize<InfoSyncRequest>(response.Data.ToString());
        }
        catch (Exception e)
        {
            Logger.Log($"Error while getting foreign data: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

    private async Task<InfoSyncRequest> GetMyData(TcpClient tcpClient, IModel peer)
    {
        try
        {
            InfoSyncRequest myData = new InfoSyncRequest();
            var blocks = await GetBlocksData();
            var transactions = await GetTransactionsData();

            myData.BlocksCount = blocks.Count;
            myData.TimeOfLastBlock = (DateTime)blocks.LastOrDefault()?.Timestamp;
            myData.LastBlockHash = blocks.LastOrDefault()?.Hash;
            myData.CountOfTransactions = transactions.Count;
            myData.TransactionIds = transactions.Select(x => x.Id).ToList();

            return myData;
        }
        catch (Exception e)
        {
            Logger.Log($"Error while getting my data: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

    private async Task<List<BlockModel>> GetBlocksData()
    {
        var modelsBlock = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
        return modelsBlock.Cast<BlockModel>().ToList();
    }

    private async Task<List<TransactionModel>> GetTransactionsData()
    {
        var modelsTx = await _dbProcessor.ProcessService<List<IModel>>(new TransactionBdService(new AppDbContext()), CommandType.GetAll);
        return modelsTx.Cast<TransactionModel>().ToList();
    }

    private async Task ValidateAndRequestBlocks(IModel peer)
    {
        var validator = new BlockChainValidator();
        var blocks = await GetBlocksData();
        var blockchainModel = new BlockchainModel { Blocks = blocks };

        var result = await validator.Validate(blockchainModel);
        if (result.Status == "Success")
        {
            Logger.Log("Blockchain validation successful, creating block request...", LogLevel.Information, Source.Client);
            await _requestFactory.CreateBlockRequest(_requestPool, peer);
        }
        else
        {
            Logger.Log("Blockchain validation failed.", LogLevel.Warning, Source.Client);
        }
    }
}
