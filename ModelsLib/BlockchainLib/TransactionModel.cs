using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelsLib.BlockchainLib;

public class TransactionModel
{
    [Key]
    public string Id { get; set; } 
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Fee { get; set; }         
    public string Signature { get; set; }
    [ForeignKey("BlockModelId")] public string? BlockId { get; set; } = null;
    [NotMapped]
    public Object Data { get; set; }
    [NotMapped]
    public SmartContractModel Contract { get; set; } = null;
}