using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelsLib.BlockchainLib;

public class BlockModel : IModel
{
    [Required]
    public string Id { get; set; }
    [Required]
    public int Index { get; set; }
    [Required]
    public DateTime Timestamp { get; set; }
    
    [Required]
    public string? Transactions { get; set; }
    [Required]
    public string? MerkleRoot { get; set; }
    [Required]
    public string? PreviousHash { get; set; }
    [Required]
    public string Hash { get; set; }
    [Required]
    public int Difficulty { get; set; }        
    [Required]
    public string Nonce { get; set; }            
    [Required]
    public string Signature { get; set; } 
    [Required]
    public int Size { get; set; }

    [NotMapped]
    public List<TransactionModel> TransactionsModel { get; set; }
}