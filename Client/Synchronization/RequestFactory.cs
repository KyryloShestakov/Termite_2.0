using System.Net.Sockets;
using System.Text.Json;
using CommonRequestsLibrary.Requests;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Requests.NetRequests;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Client.Synchronization;

public class RequestFactory
{
    private readonly IDbProcessor _dbProcessor;
    private readonly AppDbContext _appDbContext;

    public RequestFactory(IDbProcessor dbProcessor, AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
       _dbProcessor = dbProcessor;
    }

    public async Task CreateMyPeerInfoRequest(RequestPool _requestPool, IModel peer)
    {
        try
        {
            IModel model = await _dbProcessor.ProcessService<IModel>(new MyPrivatePeerInfoService(_appDbContext), CommandType.Get, new DbData(null, "default"));
            MyPrivatePeerInfoModel peerInfoModel = model as MyPrivatePeerInfoModel;
            
            PeerInfoModel knownPeer = peer as PeerInfoModel;
            
            var peerInfoRequest = new PeerInfoRequest
            {
                RecipientId = knownPeer.NodeId,
                ProtocolVersion = "1.0",
                Route = new List<string> { "node1", "node2", "node3" },
                Ttl = 10,
                SenderId = peerInfoModel.NodeId,
                PayLoad = new PayLoad
                {
                    PayloadObject = new Dictionary<string, object>()
                },
                RequestGroup = "Net",
                Method = "POST"
            };
            
            peerInfoRequest.PayLoad.PayloadObject.Add("PeerInfo", peerInfoModel);
            
            Logger.Log("Created Peer Info Request", LogLevel.Information, Source.Client);
            _requestPool.AddRequest(peerInfoRequest);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error creating Peer Info Request: {ex.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

    public async Task CreateKeyExchageRequest(RequestPool _requestPool, IModel peer)
    {
        try
        {
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            byte[] publickey = secureConnectionManager.GetPublicKeyBytes();
            string publicKey = System.Convert.ToBase64String(publickey);
            ConnectionKey connectionKey = new ConnectionKey { Key = publicKey };
            
            IModel model = await _dbProcessor.ProcessService<IModel>(new MyPrivatePeerInfoService(_appDbContext), CommandType.Get, new DbData(null, "default"));
            MyPrivatePeerInfoModel peerInfoModel = model as MyPrivatePeerInfoModel;

            PeerInfoModel knownPeer = peer as PeerInfoModel;
            var keyExchangeRequest = new KeyExchangeRequest()
            {
                RecipientId = knownPeer.NodeId,
                ProtocolVersion = "1.0",
                Route = new List<string> { "node1", "node2", "node3" },
                Ttl = 10,
                SenderId = peerInfoModel.NodeId,
                PayLoad = new PayLoad
                {
                    PayloadObject = new Dictionary<string, object>
                    {
                        { "PublicKey", connectionKey }
                    }
                },
                RequestGroup = "Net",
                Method = "POST"
            };
            
            Logger.Log("Created keyExchange Request", LogLevel.Information, Source.Client);
            _requestPool.AddRequest(keyExchangeRequest);
        }
        catch (Exception e)
        {
            Logger.Log($"Error creating KeyExchange Request: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
       
    }

    public async Task CreateTransactionRequest(RequestPool _requestPool, IModel peer)
    {
        try
        {
            List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new TransactionBdService(_appDbContext), CommandType.GetAll);
            List<TransactionModel> transactions = models.Cast<TransactionModel>().ToList();
            
            PeerInfoModel knownPeer = peer as PeerInfoModel;

            IModel myInfo = await _dbProcessor.ProcessService<IModel>(new MyPrivatePeerInfoService(_appDbContext),
                CommandType.Get, new DbData(null, "default"));
            
            MyPrivatePeerInfoModel peerInfoModel = myInfo as MyPrivatePeerInfoModel;
            
            if (transactions == null || transactions.Count == 0)
            {
                _requestPool.AddRequest(new EmptyRequest("Empty"));
            }
            else
            {
                var TypeOfTransaction = "Unconfirmed";
                foreach (var transaction in transactions)
                {
                    transaction.Data = TypeOfTransaction;
                }

                if (transactions == null || !transactions.Any())
                {
                    Logger.Log("No transactions found in SqlLite", LogLevel.Warning, Source.Client);
                }

                var transactionRequest = new TransactionRequest
                {
                    RecipientId = knownPeer.NodeId,
                    ProtocolVersion = "2.0",
                    Route = new List<string> { "node1", "node2", "node3" },
                    Ttl = 10,
                    SenderId = peerInfoModel.NodeId,
                    RequestGroup = "Blockchain",
                    Method = "POST",
                    PayLoad = new PayLoad
                    {
                        Transactions = transactions
                    }
                };
            
                Logger.Log("Created Transaction Request", LogLevel.Information, Source.Client);
                _requestPool.AddRequest(transactionRequest);
            }
           
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
            List<IModel> blocks = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(_appDbContext), CommandType.GetAll);
            if (blocks == null)
            {
                Logger.Log("Error: blocks is null", LogLevel.Error, Source.Client);
                return;
            }
            MyPrivatePeerInfoModel peerInfoModel = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(new MyPrivatePeerInfoService(_appDbContext), CommandType.Get, new DbData(null, "default"));
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

            
            var blockRequest = new BlockRequest()
            {
                RecipientId = knownPeer.NodeId,
                ProtocolVersion = "2.0",
                Route = new List<string> { "node1", "node2", "node3" },
                Ttl = 10,
                SenderId = peerInfoModel.NodeId,
                RequestGroup = "Blockchain",
                Method = "POST",
                PayLoad = new PayLoad
                {
                    Blocks = blockModels
                }
            };
            
            Logger.Log("Created block Request", LogLevel.Information, Source.Client);
            _requestPool.AddRequest(blockRequest);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
