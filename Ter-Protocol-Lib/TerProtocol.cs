using System.Runtime.Serialization;
using System.Text.Json;
using Utilities;

namespace Ter_Protocol_Lib;

public class TerProtocol<T>
{
    public TerHeader Header { get; set; }
    public TerPayload<T> Payload { get; set; }

    public TerProtocol(TerHeader header, TerPayload<T> payload)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    public virtual string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public static TerProtocol<T>? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<TerProtocol<T>>(json);
        }
        catch (Exception e)
        {
            Logger.Log($"Error while deserializing {nameof(TerProtocol<T>)}: {e.Message}", LogLevel.Error, Source.Protocol);
            throw;
        }
    }
}