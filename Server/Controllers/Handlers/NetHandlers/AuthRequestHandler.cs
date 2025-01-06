using RRLib;
using RRLib.Responses;
using SecurityLib.Authentication;

namespace Server.Controllers.Handlers.NetHandlers;

public class AuthRequestHandler : IRequestHandler
{
    private AuthService _authService;

    public AuthRequestHandler()
    {
        _authService = new AuthService();
    }
    
    public async Task<Response> HandleRequestAsync(Request request)
    { 
        switch (request.Method)
        {
            case "POST":
                return await _authService.KeyExchange(request);
            
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}