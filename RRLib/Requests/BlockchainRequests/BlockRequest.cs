using ModelsLib.BlockchainLib;
using Newtonsoft.Json;

namespace RRLib.Requests.BlockchainRequests;

public class BlockRequest : Request
{
    public BlockRequest() : base("block")
    {
    }
    
    public static BlockRequest Deserialize(string json) => JsonConvert.DeserializeObject<BlockRequest>(json);
    
    public BlockModel GetBlock()
    {
        if (PayLoad.Block != null)
        {
            return PayLoad.Block;
        }
        
        throw new InvalidOperationException("Block not found.");
    }
}