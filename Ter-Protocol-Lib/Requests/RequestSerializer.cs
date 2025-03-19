using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Utilities;

namespace Ter_Protocol_Lib.Requests;

/// <summary>
/// The RequestSerializer class handles dynamic deserialization of JSON data into objects based on message type.
/// It uses reflection to find types that implement IRequest and maps them to the TerMessageType enumeration.
/// </summary>
public static class RequestSerializer
{
    // Dictionary to store the mapping between TerMessageType and the corresponding request type
    private static readonly Dictionary<TerMessageType, Type> RequestTypes = new();

    /// <summary>
    /// Static constructor that initializes the RequestTypes dictionary by scanning the current assembly for types 
    /// that implement IRequest and mapping them to the TerMessageType enum based on the class names.
    /// </summary>
    static RequestSerializer()
    {
        // Get all types from the current assembly
        var requestTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            // Filter types that implement IRequest and are not interfaces or abstract
            .Where(t => typeof(IRequest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        // Loop through the found types
        foreach (var type in requestTypes)
        {
            // Convert class name to corresponding TerMessageType by removing "Request" suffix
            var enumName = type.Name.Replace("Request", "");
            
            // Try to parse the class name into a TerMessageType enum value
            if (Enum.TryParse(enumName, out TerMessageType messageType))
            {
                // Map the message type to the corresponding request type
                RequestTypes[messageType] = type;
            }
        }
    }

    /// <summary>
    /// Deserializes the given JSON payload into the corresponding request type based on the provided message type.
    /// </summary>
    /// <param name="messageType">The type of the message to deserialize.</param>
    /// <param name="payload">The JSON string representing the data to be deserialized.</param>
    /// <returns>The deserialized object of the corresponding type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the message type is unknown.</exception>
    public static object? DeserializeData(TerMessageType messageType, string payload)
    {
        var node = JsonNode.Parse(payload);
        var transactions = node["Data"];
        
        if (RequestTypes.TryGetValue(messageType, out var requestType))
        {
            object? obj = JsonSerializer.Deserialize(transactions, requestType);
            return obj;
        }

        throw new InvalidOperationException($"Unknown request type: {messageType}");
    }

}
