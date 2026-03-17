using ModelsLib.NetworkModels;

namespace Ter_Protocol_Lib.Requests;

public class PeerInfoRequest : IRequest
{
    public List<PeerInfoModel> Peers { get; set; }
}