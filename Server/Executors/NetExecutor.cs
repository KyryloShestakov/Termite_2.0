using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using RRLib.Requests.NetRequests;
using RRLib;
using RRLib.Responses;
using SecurityLib.Security;
using Server.Controllers;
using Server.Requests;
using StorageLib.DB.Redis;
using Utilities;

namespace Server.Executors;

/// <summary>
/// Handles the execution of network requests in the server environment.
/// </summary>
/// <remarks>
/// The `NetExecutor` class is responsible for processing TCP requests by:
/// - Parsing and deserializing the incoming request.
/// - Decrypting the payload if the request is encrypted.
/// - Routing the request based on its type (e.g., "KeyExchange", "KnownPeers").
/// - Delegating request handling to the appropriate controller or service.
/// - Logging key events and errors during the request lifecycle.
/// 
/// Features:
/// - Secure payload decryption using session keys stored in Redis.
/// - Support for multiple request types with extensible handling logic.
/// - Comprehensive error handling for JSON deserialization, socket errors, and unexpected exceptions.
/// - Asynchronous execution to support high concurrency.
/// 
/// Example Usage:
/// ```c#
/// TcpClient client = ...;
/// TcpHandler handler = ...;
/// TcpRequest request = ...;
/// var executor = new NetExecutor(client, handler, request);
/// ```
/// </remarks>
public class NetExecutor
{
    private TcpClient _tcpClient;
    private TcpHandler _tcpHandler;
    private TcpRequest _request;

    private Controller _controller;

    public NetExecutor(TcpClient tcpClient, TcpHandler tcpHandler, TcpRequest tcpRequest)
    {
        _tcpClient = tcpClient;
        _tcpHandler = tcpHandler;
        _request = tcpRequest;

        _controller = new Controller();
        _ = Execute(_tcpClient, _tcpHandler, _request);
    }

    private async Task Execute(TcpClient tcpClient, TcpHandler tcpHandler, TcpRequest tcpRequest)
    {
        try
        {
            Logger.Log($"Starting request execution for {tcpClient.Client.RemoteEndPoint}", LogLevel.Information, Source.Server);

            Request request = Request.Deserialize(tcpRequest.Message);

            SecureConnectionManager secureConnectionManager = new SecureConnectionManager();
            RedisService redisService = new RedisService();
            
            if (request.PayLoad.IsEncrypted)
            {
                Logger.Log("Encrypted Request - starting of decryption", LogLevel.Information, Source.Server);
                string sessionKey = await redisService.GetStringAsync(request.SenderId);
                byte[] sessionKeyBytes = Convert.FromBase64String(sessionKey);
                
                if (sessionKeyBytes.Length > 32)
                {
                    Logger.Log($"Session key is larger than 32 bytes, truncating it to 256 bits (32 bytes).", LogLevel.Warning, Source.Secure);
                    Logger.Log(sessionKeyBytes.Length.ToString(), LogLevel.Warning, Source.Secure);
                    Array.Resize(ref sessionKeyBytes, 32);
                }
                
                string decryptedPayload = secureConnectionManager.DecryptMessage(request.PayLoad.EncryptedPayload, sessionKeyBytes);
                PayLoad deserializedPayload = PayLoad.DeserializePayLoad(decryptedPayload);
                request.PayLoad = deserializedPayload;
            }

            Logger.Log($"Received request of type: {request.RequestType}", LogLevel.Information, Source.Server);

            switch (request.RequestType)
            {
                case "KeyExchange":
                    try
                    {
                        Logger.Log("Processing KeyExchange...", LogLevel.Information, Source.Server);

                        KeyExchangeRequest keyExchangeRequest = KeyExchangeRequest.Deserialize(tcpRequest.Message);
                        ConnectionKey publicKey = keyExchangeRequest.GetPublicKey();
                        Logger.Log($"Public key received: {publicKey.Key}", LogLevel.Information, Source.Server);

                        Response response = await _controller.HandleRequestAsync(request);
                        string responseJson = JsonSerializer.Serialize(response);

                        if (tcpClient.Connected)
                        {
                            await tcpHandler.Write(responseJson);
                            Logger.Log($"Response sent to client: {response.Status} | {response.Message}", LogLevel.Information, Source.Server);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Logger.Log($"Error processing KeyExchange: {innerEx.Message}", LogLevel.Error,
                            Source.Server);
                        if (tcpClient.Connected)
                        {
                            await tcpHandler.Write("Failed to process KeyExchange");
                        }
                    }

                    break;

                case "KnownPeers":
                    try
                    {
                        KnownPeersRequest knownPeersRequest = KnownPeersRequest.Deserialize(tcpRequest.Message);
                        Response response = await _controller.HandleRequestAsync(knownPeersRequest);
                        string responseJson = JsonSerializer.Serialize(response);
                        if (tcpClient.Connected)
                        {
                            await tcpHandler.Write(responseJson);
                            Logger.Log($"Response sent to client: {response.Message}", LogLevel.Information,
                                Source.Server);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"{e}", LogLevel.Error, Source.Server);
                        throw;
                    }

                    break;
                
                case "PeerInfo":
                {
                    try
                    {
                        PeerInfoRequest peerInfoRequest = PeerInfoRequest.Deserialize(tcpRequest.Message);
                        Response response = await _controller.HandleRequestAsync(peerInfoRequest);
                        string responseJson = JsonSerializer.Serialize(response);
                        if (tcpClient.Connected)
                        {
                            await tcpHandler.Write(responseJson);
                            Logger.Log($"Response sent to client: {response.Message}", LogLevel.Information, Source.Server);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"{e}", LogLevel.Error, Source.Server);
                        throw;
                    }
                }
                    break;

                default:
                    Logger.Log($"Unknown request type: {request.RequestType}", LogLevel.Warning, Source.Server);
                    if (tcpClient.Connected)
                    {
                        await tcpHandler.Write($"Unsupported request type: {request.RequestType}");
                    }

                    break;
            }
        }
        catch (JsonException jsonEx)
        {
            string error = $"JSON deserialization error: {jsonEx.Message}";
            Logger.Log(error, LogLevel.Error, Source.Server);
            if (tcpClient.Connected)
            {
                await tcpHandler.Write("Invalid JSON format");
            }
        }
        catch (SocketException socketEx)
        {
            string error = $"Socket error: {socketEx.Message}";
            Logger.Log(error, LogLevel.Error, Source.Server);
        }
        catch (Exception ex)
        {
            string error = $"Unexpected error: {ex.Message}";
            Logger.Log(error, LogLevel.Error, Source.Server);
            if (tcpClient.Connected)
            {
                await tcpHandler.Write("Internal Server Error");
            }
        }
        finally
        {
            if (tcpClient.Connected)
            {
                tcpClient.Close();
                Logger.Log("Connection closed.", LogLevel.Information, Source.Server);
            }
        }
    }
}