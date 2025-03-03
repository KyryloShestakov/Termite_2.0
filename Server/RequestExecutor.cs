using System.Linq.Expressions;
using System.Net.Sockets;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using RRLib;
using RRLib.Responses;
using SecurityLib.Security;
using Server.Controllers;
using Server.Requests;
using StorageLib.DB.Redis;
using Utilities;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Server.Executors;

/// <summary>
/// The `RequestExecutor` class is responsible for handling and delegating incoming TCP requests 
/// based on their request group. It processes network (`Net`) and blockchain (`Blockchain`) 
/// related requests by utilizing respective executors (`NetExecutor`, `BlockchainExecutor`).
/// 
/// Features:
/// - Initializes and manages the execution flow for specific request groups.
/// - Handles exceptions for deserialization, network errors, and unknown request types.
/// - Logs detailed information and errors for debugging purposes.
/// 
/// Components:
/// - `NetExecutor`: Handles network-related requests.
/// - `BlockchainExecutor`: Handles blockchain-related requests.
/// 
/// Example:
/// Upon receiving a request, the class determines its group and delegates it to the appropriate 
/// executor, ensuring structured and modular handling of operations.
/// </summary>
public class RequestExecutor
{
    public async Task Execute(TcpClient tcpClient, TcpHandler tcpHandler, TcpRequest tcpRequest)
    {
        Logger.Log("Request executor started.", LogLevel.Information, Source.Server);
        Controller controller = new Controller();
        try
        {
            Logger.Log($"Starting request execution for {tcpClient.Client.RemoteEndPoint}", LogLevel.Information, Source.Server);

            Request request = Request.Deserialize(tcpRequest.Message);
            if (request.PayLoad.IsEncrypted) request = await DecryptRequest(request);

            Logger.Log($"Received request of type: {request.RequestType}", LogLevel.Information, Source.Server);
            Response response = await controller.HandleRequestAsync(request);
            string responseJson = JsonSerializer.Serialize(response);
            
            await tcpHandler.Write(responseJson);
            Logger.Log($"Response sent to client: {response.Status} | {response.Message}", LogLevel.Information, Source.Server);
            
        }
        catch (JsonException jsonEx)
        {
            string error = $"JSON deserialization error: {jsonEx.Message}";
            Logger.Log(error, LogLevel.Error, Source.Server);
            await tcpHandler.Write("Invalid JSON format");
        }
        catch (Exception ex)
        {
            string error = $"Unexpected error: {ex.Message}";
            Logger.Log(error, LogLevel.Error, Source.Server);
            await tcpHandler.Write("Internal Server Error");
        }
        // finally
        // {
        //    // tcpClient.Close();
        //     Logger.Log("Connection closed.", LogLevel.Information, Source.Server);
        // }
    }

    public async Task<Request> DecryptRequest(Request request)
    {
        RedisService redisService = new RedisService();
        SecureConnectionManager secureConnectionManager = new SecureConnectionManager();

        try
        {
            if (request != null && request.PayLoad.IsEncrypted)
            {
                Logger.Log("Encrypted Request - starting of decryption", LogLevel.Information, Source.Server);
                string sessionKey = await redisService.GetStringAsync(request.SenderId);
                byte[] sessionKeyBytes = Convert.FromBase64String(sessionKey);

                if (sessionKeyBytes.Length > 32) Array.Resize(ref sessionKeyBytes, 32);

                string decryptedPayload = secureConnectionManager.DecryptMessage(request.PayLoad.EncryptedPayload, sessionKeyBytes);
                PayLoad deserializedPayload = PayLoad.DeserializePayLoad(decryptedPayload);
                if (request.RequestType == "Block") { request.PayLoad.Blocks = deserializedPayload.Blocks; }
                if (request.RequestType == "Transaction") request.PayLoad.Transactions = deserializedPayload.Transactions;
               
            }

            return request;
        }
        catch (Exception e)
        {
            Logger.Log($"{e.Message}", LogLevel.Error, Source.Server);
            throw;
        }
    }
}