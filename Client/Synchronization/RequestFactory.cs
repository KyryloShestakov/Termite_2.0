using CommonRequestsLibrary.Requests;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using SecurityLib.Security;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Ter_Protocol_Lib.Requests;
using Utilities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Client.Synchronization;

public class RequestFactory
{
    private readonly IDbProcessor _dbProcessor;

    public RequestFactory(IDbProcessor dbProcessor, AppDbContext appDbContext)
    {
       _dbProcessor = dbProcessor;
    }

    public async Task CreateKeyExchageRequest(RequestPool _requestPool, IModel peer)
    {
        try
        {
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            byte[] publickey = secureConnectionManager.GetPublicKeyBytes();
            string publicKey = System.Convert.ToBase64String(publickey);
            
            IModel model = await _dbProcessor.ProcessService<IModel>(new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));
            MyPrivatePeerInfoModel peerInfoModel = model as MyPrivatePeerInfoModel;

            PeerInfoModel knownPeer = peer as PeerInfoModel;
            
            KeyExchangeRequest keyExchangeRequest = new KeyExchangeRequest(publicKey);
            TerProtocol<object> terKeyExchangeRequest = new TerProtocol<object>(
                new TerHeader(TerMessageType.KeyExchange, knownPeer.NodeId, MethodType.Post), new TerPayload<object>(keyExchangeRequest));
            
            Logger.Log("Created keyExchange Request", LogLevel.Information, Source.Client);
            _requestPool.AddRequest(terKeyExchangeRequest);
        }
        catch (Exception e)
        {
            Logger.Log($"Error creating KeyExchange Request: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
       
    }

    public async Task CreateTransactionRequest(RequestPool _requestPool, IModel peer, List<string> transactionIds = null)
{
    try
    {
        List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new TransactionBdService(new AppDbContext()), CommandType.GetAll);
        List<TransactionModel> transactions = models.Cast<TransactionModel>().ToList();
        
        PeerInfoModel knownPeer = peer as PeerInfoModel;
        
        IModel myInfo = await _dbProcessor.ProcessService<IModel>(new MyPrivatePeerInfoService(new AppDbContext()),
            CommandType.Get, new DbData(null, "default"));
        
        MyPrivatePeerInfoModel peerInfoModel = myInfo as MyPrivatePeerInfoModel;
        
        var TypeOfTransaction = "Unconfirmed";
        foreach (var transaction in transactions)
        {
            transaction.Data = TypeOfTransaction;
        }

        if (transactions == null || !transactions.Any())
        {
            Logger.Log("No transactions found in SqlLite", LogLevel.Warning, Source.Client);
        }

        if (transactionIds != null && transactionIds.Any())
        {
            transactions = transactions.Where(t => transactionIds.Contains(t.Id)).ToList();
        }

        if (!transactions.Any())
        {
            Logger.Log("No matching transactions found", LogLevel.Warning, Source.Client);
        }

        TransactionRequest transactionRequestData = new TransactionRequest();
        transactionRequestData.Transactions = transactions;
        
        TerProtocol<object> terTransactionsRequest = new TerProtocol<object>(
            new TerHeader(TerMessageType.Transaction, knownPeer.NodeId, MethodType.Post), new TerPayload<object>(transactionRequestData));
        
        Logger.Log("Created Transaction Request", LogLevel.Information, Source.Client);
        _requestPool.AddRequest(terTransactionsRequest);
    }
    catch (Exception ex)
    {
        Logger.Log($"Error creating Transaction Request: {ex.Message}", LogLevel.Error, Source.Client);
        throw;
    }
}


    public async Task CreateBlockRequest(RequestPool _requestPool, IModel peer)
    {
        try
        {
            List<IModel> blocks = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
            if (blocks == null)
            {
                Logger.Log("Error: blocks is null", LogLevel.Error, Source.Client);
                return;
            }
            MyPrivatePeerInfoModel peerInfoModel = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));
            if (peerInfoModel == null)
            {
                Logger.Log("Error: peerInfoModel is null", LogLevel.Error, Source.Client);
                return;
            }
            PeerInfoModel knownPeer = peer as PeerInfoModel;
            if (knownPeer == null)
            {
                Logger.Log("Error: peer is null or cannot be cast to PeerInfoModel", LogLevel.Error, Source.Client);
                return;
            }
            List<BlockModel> blockModels = blocks.Cast<BlockModel>().ToList();
            
            foreach (var block in blockModels)
            {
                Logger.Log($"{block.Transactions}", LogLevel.Warning, Source.Client);
                block.TransactionsModel = JsonSerializer.Deserialize<List<TransactionModel>>(block.Transactions);
                block.Transactions = "";
            }
            
            
            BlockRequest blockRequestData = new BlockRequest();
            blockRequestData.Blocks = blockModels;
            
            TerProtocol<object> terBlockRequest = new TerProtocol<object>(
                new TerHeader(), new TerPayload<object>(blockRequestData));
            
            Logger.Log("Created block Request", LogLevel.Information, Source.Client);
            _requestPool.AddRequest(terBlockRequest);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task CreatePeerInfoRequest(RequestPool requestPool, IModel peer)
    {
        try
        {
            List<IModel> models =
                await _dbProcessor.ProcessService<List<IModel>>(new PeerInfoService(new AppDbContext()),
                    CommandType.GetAll);
            
            List<PeerInfoModel> knownPeers = models.Cast<PeerInfoModel>().ToList();
            MyPrivatePeerInfoModel peerInfoModel = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));

            var myInfo = new PeerInfoModel
            {
                NodeId = peerInfoModel.NodeId,
                IpAddress = peerInfoModel.IpAddress,
                LastSeen = peerInfoModel.LastSeen,
                NodeType = peerInfoModel.NodeType,
                Port = peerInfoModel.Port,
                SoftwareVersion = peerInfoModel.SoftwareVersion,
                Status = peerInfoModel.Status,
            };
            knownPeers.Add(myInfo);
            
            if (peerInfoModel == null)
            {
                Logger.Log("Error: peerInfoModel is null", LogLevel.Error, Source.Client);
                return;
            }
            PeerInfoModel knownPeer = peer as PeerInfoModel;
            if (knownPeer == null)
            {
                Logger.Log("Error: peer is null or cannot be cast to PeerInfoModel", LogLevel.Error, Source.Client);
                return;
            }

            TerProtocol<object> terPeerInfoRequest = new TerProtocol<object>(
                new TerHeader(TerMessageType.PeerInfo,knownPeer.NodeId,MethodType.Post), new TerPayload<object>(knownPeers));
            
            Logger.Log("Created known peers Request", LogLevel.Information, Source.Client);
            requestPool.AddRequest(terPeerInfoRequest);
        }
        catch (Exception e)
        {
            Logger.Log($"Error creating Known Peers Request: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

    public async Task CreateGetTransactionRequest(RequestPool requestPool)
    {
        Guid guid = new Guid("ab90ee9b-babe-4bcc-a41f-4bd0fee16d3b");
        TransactionRequest transactionRequest = new TransactionRequest();
        transactionRequest.Id = guid;
        TerProtocol<object> getTx = new TerProtocol<object>(new TerHeader(TerMessageType.Transaction,"ab90ee9b-babe-4bcc-a41f-4bd0fee16d3b", MethodType.Get ), new TerPayload<object>(transactionRequest));
        
        requestPool.AddRequest(getTx);
    }

    public async Task CreateGetInfoForSyncRequest(RequestPool requestPool)
    {
        TerProtocol<object> getInfo = new TerProtocol<object>(
            new TerHeader(TerMessageType.InfoSync, "ab90ee9b-babe-4bcc-a41f-4bd0fee16d3b", MethodType.Get),
            new TerPayload<object>());
        requestPool.AddRequest(getInfo);
    }


}
