namespace Ter_Protocol_Lib.Requests;

public class KeyExchangeRequest : IRequest
{
    public string Key { get; set; }

    public KeyExchangeRequest(string key)
    {
        Key = key;
    }
}