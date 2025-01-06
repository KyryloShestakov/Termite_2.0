using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Requests.NetRequests;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using Utilities;

namespace Client.Synchronization;

public class RequestFactory
{
    private readonly RedisService _redisService;
    private readonly PeerInfoService _peerInfoService;

    public RequestFactory()
    {
        _redisService = new RedisService();
        _peerInfoService = new PeerInfoService(new AppDbContext());
    }

    public async Task CreatePeerInfoRequest(RequestPool _requestPool)
    {
        try
        {
            PeerInfoModel peerInfoModel = await _peerInfoService.GetMyPeerInfo() ?? throw new InvalidOperationException();
            
            var peerInfoRequest = new PeerInfoRequest
            {
                RecipientId = "node",
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

    public async Task CreateKeyExchageRequest(RequestPool _requestPool)
    {
        try
        {
            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            byte[] publickey = secureConnectionManager.GetPublicKeyBytes();
            string publicKey = System.Convert.ToBase64String(publickey);
            ConnectionKey connectionKey = new ConnectionKey { Key = publicKey };
            
            PeerInfoModel peerInfoModel = await _peerInfoService.GetMyPeerInfo() ?? throw new InvalidOperationException();


            var keyExchangeRequest = new KeyExchangeRequest()
            {
                RecipientId = "node1",
                ProtocolVersion = "1.0",
                Route = new List<string> { "node1", "node2", "node3" },
                Ttl = 10,
                SenderId = "client123",
                //TODO Мне надо сделать в таблице известных узлов их айди 
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

    public async Task CreateTransactionRequest(RequestPool _requestPool)
    {
        try
        {
            List<TransactionModel> transactions = await _redisService.GetAllTransactionsAsync();
            
            if (transactions == null || !transactions.Any())
            {
                Logger.Log("No transactions found in Redis", LogLevel.Warning, Source.Client);
            }

            var transactionRequest = new TransactionRequest
            {
                RecipientId = "node3",
                ProtocolVersion = "2.0",
                Route = new List<string> { "node1", "node2", "node3" },
                Ttl = 10,
                SenderId = "client123",
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
        catch (Exception ex)
        {
            Logger.Log($"Error creating Transaction Request: {ex.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }
}
