using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Responses;
using SecurityLib.Authentication;
using Ter_Protocol_Lib;

namespace Server.Controllers.Handlers.NetHandlers;

public class AuthRequestHandler : IRequestHandler
{
    private AuthService _authService;

    public AuthRequestHandler()
    {
        _authService = new AuthService();
    }
    
    public async Task<Response> HandleRequestAsync(TerProtocol<IRequest> request)
    {
        TerProtocol<KeyRequest> authRequest = request.Payload.Data as TerProtocol<KeyRequest>;
        switch (request.Header.MethodType)
        {
            case MethodType.Post:
                return await _authService.KeyExchange(authRequest);
            
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}