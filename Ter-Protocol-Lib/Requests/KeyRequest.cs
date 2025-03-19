namespace Ter_Protocol_Lib.Requests;

public class KeyRequest : IRequest
{
    public string Key { get; set; }

    public KeyRequest(string key)
    {
        Key = key;
    }
}