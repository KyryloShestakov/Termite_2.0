using System.Security.Cryptography;
using System.Text;
using RRLib;
using RRLib.Requests.NetRequests;
using RRLib.Responses;
using SecurityLib.Security;
using StorageLib.DB.Redis;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace SecurityLib.Authentication;

public class KeyExchangeHandler
{
    private SecureConnectionManager _secureConnectionManager;
    private RedisService _redisService;
    private ServerResponseService _serverResponseService;

    public KeyExchangeHandler()
    {
        // Initialize dependencies
        _secureConnectionManager = new SecureConnectionManager();
        _redisService = new RedisService();
        _serverResponseService = new ServerResponseService(); 
    }

    public async Task<Response> EstablishSessionWithNodeAsync(TerProtocol<KeyRequest> request)
    {
        // Validate the input request
        if (request == null)
        {
            Logger.Log("Node information cannot be null or empty.", LogLevel.Error, Source.Secure);
            throw new ArgumentException("Node information cannot be null or empty.", nameof(request));
        }

        try
        {
            Logger.Log("KeyExchange is starting", LogLevel.Information, Source.Secure);
            
            // Generate a session key
            byte[] sessionKey = await Task.Run(() => _secureConnectionManager.GenerateSessionKey());

            // Cast request to KeyExchangeRequest to extract public key
            if (request == null)
            {
                Logger.Log("Invalid request type for key exchange.", LogLevel.Error, Source.Secure);
                throw new ArgumentException("Invalid request type for key exchange.");
            }

            // Extract and convert public key
            string publicKey = request.Payload.Data.Key;
            RSAParameters recipientPublicKey;
            try
            {
                recipientPublicKey = SecureConnectionManager.ConvertFromBase64ToRSAParameters(publicKey);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error converting public key: {ex.Message}", LogLevel.Error, Source.Secure);
                throw new ArgumentException("Failed to convert public key.", ex);
            }

            // Encrypt the session key
            byte[] encryptedSessionKey = _secureConnectionManager.EncryptSessionKey(sessionKey, recipientPublicKey);

            // Store the encrypted session key in Redis
            try
            {
                await _redisService.SetStringAsync(request.Header.SenderId, Convert.ToBase64String(encryptedSessionKey));
                Logger.Log($"New session key added to Redis successfully. {encryptedSessionKey.Length} bytes", LogLevel.Information, Source.Secure);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to add session key to Redis: {ex.Message}", LogLevel.Error, Source.Secure);
                throw new InvalidOperationException("Failed to store session key in Redis.", ex);
            }

            // Get the public key of this node
            byte[] myPublicKey = _secureConnectionManager.GetPublicKeyBytes();
            
            // Prepare the response data
            var data = new 
            {
                sessionKey = encryptedSessionKey,
                publicKey = myPublicKey
            };

            // Return the response indicating success
            Response response = _serverResponseService.GetResponse(true, "Successfully established session key.", data);
            return response;
        }
        catch (Exception ex)
        {
            // Log and rethrow any errors that occur during session establishment
            Logger.Log($"Error establishing session with node: {ex.Message}", LogLevel.Error, Source.Secure);
            throw new InvalidOperationException("An error occurred while establishing session.", ex);
        }
    }
}
