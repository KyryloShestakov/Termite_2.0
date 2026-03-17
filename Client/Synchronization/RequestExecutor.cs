using System.Net.Sockets;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using RRLib.Requests.NetRequests;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using Ter_Protocol_Lib.Requests;
using Utilities;
using PeerInfoRequest = Ter_Protocol_Lib.Requests.PeerInfoRequest;

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

    private async Task HandleRequest(TcpClient tcpClient, NetworkStream networkStream, TerProtocol<object> request)
    {
        string requestEnc = await EncryptRequest(request);
        request.Payload.Data = requestEnc;
        string jsonKey = request.Serialize();
        await _requestHandler.SendRequestAsync(tcpClient, jsonKey, networkStream);
        await _responseHandler.ReceiveResponse(tcpClient);
    }
    

    public async Task<string> EncryptRequest(TerProtocol<object> request)
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
            byte[] sessionKeyBytes = Convert.FromBase64String(sessionkeystr);
            if (sessionKeyBytes.Length > 32)
            {
                Array.Resize(ref sessionKeyBytes, 32); 
            }
           
            string serializedTransactions = JsonConvert.SerializeObject(request.Payload);
            byte[] encryptedMessage = _secureConnectionManager.EncryptMessage(serializedTransactions, sessionKeyBytes);
            string encryptedMessageBase64 = Convert.ToBase64String(encryptedMessage);
           
            Logger.Log($"Encrypted message: {Convert.ToBase64String(encryptedMessage)}", LogLevel.Information, Source.Client);
            
            return encryptedMessageBase64;
        }
        catch (Exception e)
        {
            Logger.Log($"Error in request encryption: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }




}