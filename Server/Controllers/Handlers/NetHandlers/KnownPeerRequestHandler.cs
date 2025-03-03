using PeerLib.Services;
using RRLib;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;

namespace Server.Controllers.Handlers.NetHandlers;

public class KnownPeerRequestHandler : IRequestHandler
{
    private InfoPeerService _infoPeerService { get; }
    public KnownPeerRequestHandler()
    {
        _infoPeerService = new InfoPeerService();
    }
    
    public async Task<Response> HandleRequestAsync(Request request)
    {
        switch (request.Method)
        {
            case "GET":
                 return await _infoPeerService.GetPeers();
            case "POST":
                return await _infoPeerService.PostPeerInfo(request);
            case "UPDATE":
                return await _infoPeerService.UpdatePeer(request);
            case "DELETE":
                return await _infoPeerService.DeletePeer(request);
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}