using System.Reflection;
using System.Text.Json;

namespace Ter_Protocol_Lib;

public static class RequestSerializer
{
    private static readonly Dictionary<TerMessageType, Type> RequestTypes = new();

    static RequestSerializer()
    {
        var requestTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IRequest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in requestTypes)
        {
            var enumName = type.Name.Replace("Request", "");
            if (Enum.TryParse(enumName, out TerMessageType messageType))
            {
                RequestTypes[messageType] = type;
            }
        }
    }

    public static object? DeserializeData(TerMessageType messageType, string payload)
    {
        if (RequestTypes.TryGetValue(messageType, out var requestType))
        {
            return JsonSerializer.Deserialize(payload, requestType);
        }
        throw new InvalidOperationException($"Unknown request type: {messageType}");
    }
}