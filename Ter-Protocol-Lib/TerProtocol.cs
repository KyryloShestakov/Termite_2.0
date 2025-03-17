using System.Text.Json; // Import System.Text.Json for JSON serialization and deserialization
using Utilities; // Import custom utilities, likely containing logging functionality

namespace Ter_Protocol_Lib; // Defines the namespace for organizing code in the Ter_Protocol_Lib library

/// <summary>
/// The TerProtocol<T> class represents a protocol message consisting of a header and a payload.
/// It supports serialization and deserialization using System.Text.Json.
/// </summary>
/// <typeparam name="T">The type of data contained within the payload.</typeparam>
public class TerProtocol<T>
{
    /// <summary>
    /// The protocol message header, containing metadata.
    /// </summary>
    public TerHeader Header { get; set; }

    /// <summary>
    /// The payload of the protocol message, containing the actual data.
    /// </summary>
    public TerPayload<T> Payload { get; set; }

    /// <summary>
    /// Constructor for creating a protocol message with a specified header and payload.
    /// Throws an exception if either parameter is null.
    /// </summary>
    /// <param name="header">The header of the protocol message.</param>
    /// <param name="payload">The payload containing the data.</param>
    public TerProtocol(TerHeader header, TerPayload<T> payload)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    /// <summary>
    /// Serializes the protocol message to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the protocol message.</returns>
    public virtual string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a JSON string into a TerProtocol<T> object.
    /// If deserialization fails, logs an error and rethrows the exception.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A TerProtocol<T> object if deserialization is successful; otherwise, null.</returns>
    public static TerProtocol<T>? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<TerProtocol<T>>(json);
        }
        catch (Exception e)
        {
            // Logs the deserialization error and rethrows the exception
            Logger.Log($"Error while deserializing {nameof(TerProtocol<T>)}: {e.Message}", LogLevel.Error, Source.Protocol);
            throw;
        }
    }
}
