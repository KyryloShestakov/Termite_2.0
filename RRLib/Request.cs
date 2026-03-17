using System.Text.Json;
using RRLib.Requests.NetRequests;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using RRLib.Requests.BlockchainRequests;
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
                "Block" => JsonSerializer.Deserialize<BlockRequest>(json),
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
    public List<PeerInfoModel> KnownPeers { get; set; } = null;
    public byte[] EncryptedPayload { get; set; } = null;
    public List<TransactionModel> Transactions { get; set; } = null;
    
    public List<BlockModel> Blocks { get; set; } = null;
    public BlockModel Block { get; set; } = null;

    public bool IsEncrypted => EncryptedPayload?.Length > 0;

    public PayLoad() { }

    public PayLoad(Dictionary<string, object> payloadObject)
    {
        PayloadObject = new Dictionary<string, object>();
        PayloadObject = payloadObject ?? throw new ArgumentNullException(nameof(payloadObject));
    }

    public PayLoad(List<PeerInfoModel> knownPeers)
    {
        KnownPeers = new List<PeerInfoModel>();
        KnownPeers = knownPeers ?? throw new ArgumentNullException(nameof(knownPeers));
    }

    public PayLoad(List<TransactionModel> transactions)
    {
        Transactions = new List<TransactionModel>();
        Transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
    }

    public PayLoad(List<BlockModel> blocks)
    {
        Blocks = new List<BlockModel>();
        Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
    }

    public PayLoad(BlockModel block)
    {
        Block = block ?? throw new ArgumentNullException(nameof(block));
    }


    public virtual string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }
    
   
    public static PayLoad? DeserializePayLoad(string decryptedPayload)
    {
        if (string.IsNullOrWhiteSpace(decryptedPayload))
        {
            Logger.Log("Decrypted payload is empty or null", LogLevel.Warning, Source.Server);
            return null;
        }

        try
        {
            Logger.Log($"Raw decrypted JSON: {decryptedPayload}", LogLevel.Information, Source.Server);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<PayLoad>(decryptedPayload, options) 
                   ?? throw new JsonException("Deserialized PayLoad is null.");
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
