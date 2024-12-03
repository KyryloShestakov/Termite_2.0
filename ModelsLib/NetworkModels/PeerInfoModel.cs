using System.ComponentModel.DataAnnotations.Schema;

namespace ModelsLib.NetworkModels;

public class PeerInfoModel
{
    public int Id { get; set; }
    public string NodeId { get; set; }

    public string? IpAddress { get; set; }

    public int? Port { get; set; }

    public DateTime? LastSeen { get; set; }

    public NodeStatus? Status { get; set; }
    public List<string>? Neighbors { get; set; }

    public string? PublicKey { get; set; }

    public string? SessionKey { get; set; } = null;

    public string? SoftwareVersion { get; set; }
    public string? NodeType { get; set; } = null;

    public int? TransactionCount { get; set; }
    [NotMapped]
    public Dictionary<string, string>? Metadata { get; set; }

    public PeerInfoModel()
    {
        Neighbors = new List<string>();
        Metadata = new Dictionary<string, string>();
    }
}

public enum NodeStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Unreachable = 4
}
