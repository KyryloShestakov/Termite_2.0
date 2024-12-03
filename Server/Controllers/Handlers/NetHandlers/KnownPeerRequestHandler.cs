using PeerLib.Services;
using RRLib;
using RRLib.Responses;

namespace Server.Controllers.Handlers.NetHandlers;

public class KnownPeerRequestHandler : IRequestHandler
{
    private KnownPeersService _knownPeersService { get; }
    public KnownPeerRequestHandler()
    {
        _knownPeersService = new KnownPeersService();
    }
    
    public async Task<Response> HandleRequestAsync(Request request)
    {
        switch (request.Method)
        {
            case "GET":
                return await _knownPeersService.GetKnownPeers(request);
            case "POST":
                return await _knownPeersService.PostKnownPeers(request);
            case "UPDATE":
                return await _knownPeersService.UpdateKnownPeers(request);
            case "DELETE":
                return await _knownPeersService.DeleteKnownPeers(request);
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}