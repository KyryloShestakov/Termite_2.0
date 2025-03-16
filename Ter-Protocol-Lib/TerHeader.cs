namespace Ter_Protocol_Lib;

public class TerHeader
{
    public TerMessageType MessageType { get; set; }
    public string PublicKey { get; set; }

    public TerHeader(TerMessageType type, string publicKey)
    {
        MessageType = type;
        PublicKey = publicKey;
    }
}