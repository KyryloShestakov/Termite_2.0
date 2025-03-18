using ModelsLib.NetworkModels;

namespace Ter_Protocol_Lib;

public class PeerInfoRequest : IRequest
{
    public List<PeerInfoModel> Peers { get; set; }
}