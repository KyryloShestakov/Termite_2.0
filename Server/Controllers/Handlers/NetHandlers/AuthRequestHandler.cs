using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Responses;
using SecurityLib.Authentication;
using Ter_Protocol_Lib.Requests;

namespace Server.Controllers.Handlers.NetHandlers;

public class AuthRequestHandler : IRequestHandler
{
    private AuthService _authService;

    public AuthRequestHandler()
    {
        _authService = new AuthService();
    }
    
    public async Task<Response> HandleRequestAsync(TerProtocol<object> request)
    {
        switch (request.Header.MethodType)
        {
            case MethodType.Post:
                // return await _authService.KeyExchange(authRequest);
            
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}