using PeerLib.Services;
using RRLib;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using Ter_Protocol_Lib;

namespace Server.Controllers.Handlers.NetHandlers;

public class PeerInfoHandler : IRequestHandler
{
    private InfoPeerService _infoPeerService { get; set; }
    public PeerInfoHandler()
    {
        _infoPeerService = new InfoPeerService();
    }
    
    public async Task<Response> HandleRequestAsync(TerProtocol<IRequest> request)
    {
        TerProtocol<PeerInfoRequest> terRequest = request.Payload.Data as TerProtocol<PeerInfoRequest>;
        TerProtocol<DataRequest<string>> terDataRequest = request.Payload.Data as TerProtocol<DataRequest<string>>; 
        switch (request.Header.MethodType)
        {
            case MethodType.Get:
                return await _infoPeerService.GetPeerById(terDataRequest);
            case MethodType.GetAll:
                return await _infoPeerService.GetPeers();
            case MethodType.Post:
                return await _infoPeerService.PostPeerInfo(terRequest);
            case MethodType.Update:
                return await _infoPeerService.UpdatePeer(terRequest);
            case MethodType.Delete:
                return await _infoPeerService.DeletePeer(terDataRequest);
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}