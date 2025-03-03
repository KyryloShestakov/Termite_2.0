using ModelsLib.BlockchainLib;
using Newtonsoft.Json;

namespace RRLib.Requests.BlockchainRequests;

public class BlockRequest : Request
{
    public BlockRequest() : base("Block")
    {
    }
    
    public static BlockRequest Deserialize(string json) => JsonConvert.DeserializeObject<BlockRequest>(json);
    
    public BlockModel GetBlock()
    {
        if (PayLoad.Blocks != null)
        {
            return PayLoad.Block;
        }
        
        throw new InvalidOperationException("Block not found.");
    }
    
    public List<BlockModel> GetBlocks()
    {
        if (PayLoad.Blocks != null)
        {
            return PayLoad.Blocks;
        }
        
        throw new InvalidOperationException("Block not found.");
    }
}