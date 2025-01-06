using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using RRLib;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RRLib.Requests.NetRequests;

public class KnownPeersRequest : Request
{
    public KnownPeersRequest() : base("KnownPeers") { }
    
    public static KnownPeersRequest Deserialize(string json) => JsonConvert.DeserializeObject<KnownPeersRequest>(json) ?? throw new InvalidOperationException();
    
    public List<KnownPeersModel> GetKnownPeers()
    {
        if (PayLoad.KnownPeers != null && PayLoad.KnownPeers.Any())
        {
            return PayLoad.KnownPeers;
        }
            
        throw new InvalidOperationException("KnownPeers not found in payload.");
    }
}