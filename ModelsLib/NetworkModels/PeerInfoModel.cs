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
    
    public string? SoftwareVersion { get; set; }
    public string? NodeType { get; set; } = null;
    
    
}

public enum NodeStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Unreachable = 4
}
