using System.Text.Json;
using Newtonsoft.Json;
using RRLib;

namespace CommonRequestsLibrary.Requests.NetRequests;

public class KeyExchangeRequest : Request
{
    public KeyExchangeRequest() : base("KeyExchange") { }
    
    public static KeyExchangeRequest Deserialize(string json) => JsonConvert.DeserializeObject<KeyExchangeRequest>(json);
    
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