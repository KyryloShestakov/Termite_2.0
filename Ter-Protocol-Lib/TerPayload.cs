using Newtonsoft.Json;

namespace Ter_Protocol_Lib.Requests;

/// <summary>
/// The TerPayload<T> class is designed to wrap data (Data) into a generic structure.
/// Supports serialization and deserialization using Newtonsoft.Json.
/// </summary>
/// <typeparam name="T">The type of data stored in the payload.</typeparam>
public class TerPayload<T>
{
    /// <summary>
    /// The main content of the payload (generic type).
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Constructor used for JSON deserialization. 
    /// Ensures the object is properly restored from JSON.
    /// </summary>
    /// <param name="data">The data to be stored in the payload.</param>
    [JsonConstructor] // Specifies which constructor Newtonsoft.Json should use during deserialization
    public TerPayload(T data)
    {
        Data = data;
    }

    /// <summary>
    /// A parameterless constructor, required for proper serialization 
    /// and object creation without arguments.
    /// </summary>
    public TerPayload() { }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }
}
