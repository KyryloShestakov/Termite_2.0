using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib.NetworkModels;
using Newtonsoft.Json.Linq;
using RRLib.Requests.BlockchainRequests;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using Ter_Protocol_Lib.Requests;
using Utilities;
using TransactionRequest = Ter_Protocol_Lib.Requests.TransactionRequest;

namespace Server.Controllers;

public class RequestManager
{
    private readonly IDbProcessor _dbProcessor;
    private readonly RedisService _redisService;
    private readonly SecureConnectionManager _secureConnectionManager;

    public RequestManager()
    {
        _dbProcessor = new DbProcessor();
        _redisService = new RedisService();
        _secureConnectionManager = new SecureConnectionManager();
    }

    public async Task<string> DecryptRequest(TerPayload<string> terProtocol)
    {
        try
        {
            var myInfo = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(
                new MyPrivatePeerInfoService(new AppDbContext()), 
                CommandType.Get, 
                new DbData(null, "default")
            );

            Logger.Log("Starting decryption process", LogLevel.Information, Source.Server);

            string sessionKey = await _redisService.GetStringAsync(myInfo.NodeId);
            if (string.IsNullOrEmpty(sessionKey))
            {
                throw new InvalidOperationException("Session key not found in Redis.");
            }

            byte[] sessionKeyBytes;
            try
            {
                sessionKeyBytes = Convert.FromBase64String(sessionKey);
                if (sessionKeyBytes.Length > 32) Array.Resize(ref sessionKeyBytes, 32);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Session key is not a valid Base64 string.");
            }
           
            
            byte[] encryptedData = Convert.FromBase64String(terProtocol.Data);
            
            string decryptedPayload = _secureConnectionManager.DecryptMessage(encryptedData, sessionKeyBytes);

            Logger.Log("Decryption successful", LogLevel.Information, Source.Server);
            
            Logger.Log($"Decrypted data: {decryptedPayload}", LogLevel.Information, Source.Server);
            
            return decryptedPayload;
        }
        catch (FormatException fe)
        {
            Logger.Log($"Invalid Base64 format: {fe.Message}", LogLevel.Error, Source.Server);
            throw;
        }
        catch (Exception e)
        {
            Logger.Log($"Decryption error: {e.Message}\n{e.StackTrace}", LogLevel.Error, Source.Server);
            throw;
        }
    }

}