using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelsLib.BlockchainLib;

namespace Ter_Protocol_Lib.Requests;

public class TransactionRequest : IRequest
{
    [JsonPropertyName("Transactions")]
    public List<TransactionModel> Transactions { get; set; }
    [JsonPropertyName("Id")]
    public Guid? Id { get; set; }

    public TransactionRequest()
    {
        Transactions = new List<TransactionModel>();
    }
}