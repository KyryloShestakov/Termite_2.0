using EnumsLib;

namespace ModelsLib.NetworkModels;

public class MyPrivatePeerInfoModel : IModel
{
    public int Id { get; set; }
    public string NodeId { get; set; }

    public string? IpAddress { get; set; }

    public int? Port { get; set; }

    public DateTime? LastSeen { get; set; }

    public NodeStatus? Status { get; set; }
    
    public string? SoftwareVersion { get; set; }
    public string? NodeType { get; set; } = null;
    public string AddressWallet { get; set; }
    public string PrivateKey { get; set; }
    public string PublicKey { get; set; }
    

}

