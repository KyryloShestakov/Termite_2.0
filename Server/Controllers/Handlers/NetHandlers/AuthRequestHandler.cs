using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Responses;
using SecurityLib.Authentication;
using Ter_Protocol_Lib.Requests;
using Utilities;

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
        
        string json = request.Payload.Serialize();
        Logger.Log(json, LogLevel.Warning, Source.Server);
        var obj = RequestSerializer.DeserializeData(request.Header.MessageType,json);
        
        switch (request.Header.MethodType)
        {
            case MethodType.Post:
                Response responseKey = await _authService.KeyExchange(request);
                return responseKey;
            
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}