using RRLib;
using RRLib.Responses;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace SecurityLib.Authentication;

public class AuthService
{
    private KeyExchangeHandler _keyExchangeHandler;

    // Constructor initializes KeyExchangeHandler to handle key exchange operations.
    public AuthService()
    {
        _keyExchangeHandler = new KeyExchangeHandler();
    }

    // KeyExchange method calls the key exchange logic from the KeyExchangeHandler.
    // It passes the request and returns the response received from the handler.
    // In case of an error, it catches exceptions and returns an empty response.
    public async Task<Response> KeyExchange(TerProtocol<KeyRequest> request)
    {
        try
        {
            // Attempt to establish a session key with the node via key exchange
            Response response = await _keyExchangeHandler.EstablishSessionWithNodeAsync(request);
            return response;
        }
        catch (Exception ex)
        {
            // Log error and return an empty response if key exchange fails
            Logger.Log($"Error during key exchange: {ex.Message}", LogLevel.Error, Source.Secure);
            return new Response();  // Returning an empty response indicating failure
        }
    }
}