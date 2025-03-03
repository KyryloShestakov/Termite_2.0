using System.Net.Sockets;
using Client.Synchronization;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib.NetworkModels;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Requests.NetRequests;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using Utilities;

namespace Client;

public class RequestExecutor
{
    private RequestHandler _requestHandler;
    private ResponseHandler _responseHandler;

    public RequestExecutor()
    {
        _requestHandler = new RequestHandler();
        _responseHandler = new ResponseHandler();
    }

    public async Task StartExecution(TcpClient tcpClient, RequestPool _requestPool)
    {
        try
        {
            Logger.Log("Starting request processing", LogLevel.Information, Source.Client);

            var requests = _requestPool.GetAllRequests();

            if (requests.Count == 0)
            {
                Logger.Log("No requests available", LogLevel.Warning, Source.Client);
                return;
            }

            Logger.Log($"Request pool contains {requests.Count} requests.", LogLevel.Information, Source.Client);
            NetworkStream networkStream = tcpClient.GetStream();

            List<Task> requestTasks = new List<Task>();

            foreach (var request in requests)
            {
                requestTasks.Add(HandleRequest(tcpClient, networkStream, request));
            }

            await Task.WhenAll(requestTasks);
        }
        catch (Exception e)
        {
            Logger.Log($"Error in request execution: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

    private async Task HandleRequest(TcpClient tcpClient, NetworkStream networkStream, Request request)
    {

        switch (request.RequestType)
        {
            case "PeerInfo":
                PeerInfoRequest peerInfoRequest = request as PeerInfoRequest;
                string peerInfoRequestJson = peerInfoRequest.Serialize();
                string peerInfoEncrypted = await EncryptRequest(peerInfoRequest);
                await _requestHandler.SendRequestAsync(tcpClient, peerInfoEncrypted, networkStream);
                await _responseHandler.ReceiveResponse(tcpClient);
                break;
            case "KeyExchange":
                KeyExchangeRequest keyExchangeRequest = request as KeyExchangeRequest;
                string keyExchangeRequestJson = keyExchangeRequest.Serialize();
                await _requestHandler.SendRequestAsync(tcpClient, keyExchangeRequestJson, networkStream);
                await _responseHandler.ReceiveResponse(tcpClient);
                break;
            case "KnownPeers":
                KnownPeersRequest knownPeersRequest = request as KnownPeersRequest;
                string knownPeersRequestJson = await EncryptRequest(knownPeersRequest);
                await _requestHandler.SendRequestAsync(tcpClient, knownPeersRequestJson, networkStream);
                await _responseHandler.ReceiveResponse(tcpClient);
                break;
            case "Block":
                BlockRequest blockRequest = request as BlockRequest;
                string blockRequestJsonEncrypted = await EncryptRequest(blockRequest);
                await _requestHandler.SendRequestAsync(tcpClient, blockRequestJsonEncrypted, networkStream);
                await _responseHandler.ReceiveResponse(tcpClient);
                break;
            case "Transaction":
                TransactionRequest transactionRequest = request as TransactionRequest;
                string transactionRequestEncrypted = await EncryptRequest(transactionRequest);
                await _requestHandler.SendRequestAsync(tcpClient, transactionRequestEncrypted, networkStream);
                await _responseHandler.ReceiveResponse(tcpClient);
                break;
        }
    }
    
    private static async Task<string> EncryptRequest(Request request)
    {
        SecureConnectionManager _secureConnectionManager = new SecureConnectionManager();
        try
        {
            Logger.Log($"Encrypting request {request.Serialize()}", LogLevel.Information, Source.Client);
            RedisService redisService = new RedisService();
            IDbProcessor _dbProcessor = new DbProcessor();
            MyPrivatePeerInfoModel myInfo = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(
                new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));
            
            
            string sessionkeystr = await redisService.GetStringAsync(myInfo.NodeId);
            if (sessionkeystr == null)
            {
                byte[] sessionkey = _secureConnectionManager.GenerateSessionKey();   
            }
            byte[] sessionkeybytes = Convert.FromBase64String(sessionkeystr);
            if (sessionkeybytes.Length > 32)
            {
                Array.Resize(ref sessionkeybytes, 32); 
            }
           
            byte[] encryptedMessage = _secureConnectionManager.EncryptMessage(request.PayLoad.Serialize(), sessionkeybytes); 
            request.PayLoad.EncryptedPayload = encryptedMessage;
           
            Logger.Log($"Encrypted message: {Convert.ToBase64String(encryptedMessage)}", LogLevel.Information, Source.Client);
            request.PayLoad.Transactions = null;
            request.PayLoad.Blocks = null;
            
            string jsonRequest = request.Serialize();
           
           return jsonRequest;
        }
        catch (Exception e)
        {
            Logger.Log($"Error in request encryption: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }




}