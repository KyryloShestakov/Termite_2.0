using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelsLib.BlockchainLib;

public class TransactionModel : IModel
{
    public string Id { get; set; } 
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Fee { get; set; }         
    public string Signature { get; set; }
    public string? BlockId { get; set; } = null;
    [NotMapped]
    public object? Data { get; set; } = null;
    [NotMapped]
    public SmartContractModel? Contract { get; set; } = null;

    public string PublicKey { get; set; }
}