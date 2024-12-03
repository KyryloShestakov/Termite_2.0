using System.Text.Json;
using System.Transactions;
using CommonRequestsLibrary.Requests.NetRequests;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using StorageLib.DB.Redis;
using Utilities;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RRLib;

public abstract class Request
{
    public string RecipientId { get; set; }
    public string ProtocolVersion { get; set; }
    public string RequestType { get; }
    public DateTime Timestamp { get; }
    public List<string> Route { get; set; }
    public int Ttl { get; set; }
    public string SenderId { get; set; }
    public PayLoad PayLoad { get; set; }
    public string RequestGroup { get; set; }
    public string Method { get; set; }

    protected Request(string requestType)
    {
        RequestType = requestType;
        Timestamp = DateTime.Now;
    }

    public virtual string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public static Request? Deserialize(string json)
    {
        try
        {
            var document = JsonDocument.Parse(json);
            string requestType = document.RootElement.GetProperty("RequestType").GetString();
            
            return requestType switch
            {
                "InitialConnection" => JsonSerializer.Deserialize<InitialConnectionRequest>(json),
                "KeyExchange" => JsonSerializer.Deserialize<KeyExchangeRequest>(json),
                "PeerInfo" => JsonSerializer.Deserialize<PeerInfoRequest>(json),
                "KnownPeers" => JsonSerializer.Deserialize<KnownPeersRequest>(json),
                "Transaction" => JsonSerializer.Deserialize<TransactionRequest>(json),
                _ => throw new InvalidOperationException($"Unknown request type: {requestType}")
            };
        }
        catch (JsonException jsonEx)
        {
            Logger.Log($"JSON Deserialization error: {jsonEx.Message}", LogLevel.Error, Source.Server);
            return null;
        }
        catch (Exception ex)
        {
            Logger.Log($"Unexpected error during deserialization: {ex.Message}", LogLevel.Error, Source.Server);
            return null;
        }
    }
}

public class PayLoad
{
    public Dictionary<string, object> PayloadObject { get; set; } = null;
    public List<KnownPeersModel> KnownPeers { get; set; } = null;
    public byte[] EncryptedPayload { get; set; } = null;
    public List<TransactionModel> Transactions { get; set; } = null;
    
    public BlockModel Block { get; set; } = null;

    public bool IsEncrypted => EncryptedPayload?.Length > 0;

    public PayLoad() { }

    public PayLoad(Dictionary<string, object> payloadObject)
    {
        PayloadObject = new Dictionary<string, object>();
        PayloadObject = payloadObject ?? throw new ArgumentNullException(nameof(payloadObject));
    }

    public PayLoad(List<KnownPeersModel> knownPeers)
    {
        KnownPeers = new List<KnownPeersModel>();
        KnownPeers = knownPeers ?? throw new ArgumentNullException(nameof(knownPeers));
    }

    public PayLoad(List<TransactionModel> transactions)
    {
        Transactions = new List<TransactionModel>();
        Transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
    }

    public PayLoad(BlockModel block)
    {
        Block = block ?? throw new ArgumentNullException(nameof(block)); 
    }


    public virtual string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }
    
   
    public static PayLoad DeserializePayLoad(string decryptedPayload)
    {
        try
        {
            PayLoad deserializedPayload = JsonSerializer.Deserialize<PayLoad>(decryptedPayload);

            return deserializedPayload;
        }
        catch (JsonException jsonEx)
        {
            Logger.Log($"JSON Deserialization error for PayLoad: {jsonEx.Message}", LogLevel.Error, Source.Server);
            return null;
        }
        catch (Exception ex)
        {
            Logger.Log($"Unexpected error during PayLoad deserialization: {ex.Message}", LogLevel.Error, Source.Server);
            return null;
        }
    }
    

}

public class ConnectionKey
{
    public string Key { get; set; }
    
}
