using System.Text.Json.Serialization;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Ter_Protocol_Lib;

public class TerPayload<T>
{
    public T Data { get; set; }
    public byte[] EncryptedPayload { get; set; }
    public bool IsEncrypted => EncryptedPayload?.Length > 0;

    public TerPayload(T data)
    {
        Data = data;
    }

    public static T DeserializePayload<T>(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload) 
               ?? throw new InvalidOperationException("Deserialization failed");
    }

    public static TerPayload<T> FromJson(string json)
    {
        return JsonSerializer.Deserialize<TerPayload<T>>(json) 
               ?? throw new InvalidOperationException("Invalid JSON format");
    }
        
    public static object? DeserializeData(TerMessageType terMessageType, string payload)
    {
        return terMessageType switch
        {
            TerMessageType.Handshake => JsonSerializer.Deserialize<HandshakeRequest>(payload),
            TerMessageType.Transaction => JsonSerializer.Deserialize<TransactionRequest>(payload),
            TerMessageType.Block => JsonSerializer.Deserialize<BlockRequest>(payload),
            _ => throw new InvalidOperationException($"Unknown request type: {terMessageType}")
        };
    }
}


public class HandshakeRequest
{
    public string message = "hello";

    public override string ToString()
    {
        return message;
    }
}

public class TransactionRequest
{ 
    public List<TransactionModel> Transactions { get; set; }
}

public class BlockRequest
{
    public List<BlockModel> Blocks { get; set; }
}
