using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelsLib.NetworkModels;
[Table("PeersList")]
public class KnownPeersModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public string Type { get; set; }
    public NodeStatus Status { get; set; }
}