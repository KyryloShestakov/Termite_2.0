using System.Text.Json;
using System.Transactions;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using RRLib;

namespace RRLib.Requests.BlockchainRequests;

public class TransactionRequest : Request
{
    public TransactionRequest() : base("Transaction")
    {
        
    }
    
    public static TransactionRequest Deserialize(string json) => JsonConvert.DeserializeObject<TransactionRequest>(json);
    
    public List<TransactionModel> GetTransactions()
    {
        if (PayLoad.Transactions != null && PayLoad.Transactions.Count > 0)
        {
           return PayLoad.Transactions;
        }
        
        throw new InvalidOperationException("Transactions not found.");
    }

}