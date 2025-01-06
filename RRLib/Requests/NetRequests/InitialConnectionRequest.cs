using System.Text.Json;
using Newtonsoft.Json;
using RRLib;

namespace RRLib.Requests.NetRequests;

public class InitialConnectionRequest : Request
{
    public static InitialConnectionRequest Deserialize(string json) => JsonConvert.DeserializeObject<InitialConnectionRequest>(json);

    public InitialConnectionRequest() : base("InitialConnection")
    {
    }
    public ConnectionKey GetPublicKey()
    {
        if (PayLoad.PayloadObject.TryGetValue("PublicKey", out var json))
        {
            string? peerInfoJsonString = json is JsonElement jsonElement 
                ? jsonElement.GetRawText() 
                : json.ToString();

            return JsonConvert.DeserializeObject<ConnectionKey>(peerInfoJsonString);
        }
        
        throw new InvalidOperationException("Key not found in payload.");
    }


}