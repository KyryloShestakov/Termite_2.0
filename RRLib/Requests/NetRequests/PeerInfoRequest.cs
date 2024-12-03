using System.Text.Json;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using RRLib;

namespace CommonRequestsLibrary.Requests.NetRequests;

public class PeerInfoRequest : Request
{
    public PeerInfoRequest() : base("PeerInfo") { }
    public static PeerInfoRequest Deserialize(string json) => JsonConvert.DeserializeObject<PeerInfoRequest>(json);
    
    public PeerInfoModel GetPeerInfo()
    {
        if (PayLoad.PayloadObject.TryGetValue("PeerInfo", out var peerInfoJson))
        {
            string? peerInfoJsonString = peerInfoJson is JsonElement jsonElement 
                ? jsonElement.GetRawText() 
                : peerInfoJson.ToString();

            return JsonConvert.DeserializeObject<PeerInfoModel>(peerInfoJsonString);
        }
        
        throw new InvalidOperationException("PeerInfo not found in payload.");
    }
    
}