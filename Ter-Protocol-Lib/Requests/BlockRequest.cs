using ModelsLib.BlockchainLib;

namespace Ter_Protocol_Lib.Requests;

public class BlockRequest : IRequest
{
    public List<BlockModel> Blocks { get; set; }

    public BlockRequest()
    {
        Blocks = new List<BlockModel>();
    }
}