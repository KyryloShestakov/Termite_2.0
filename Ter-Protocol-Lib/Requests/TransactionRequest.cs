using ModelsLib.BlockchainLib;

namespace Ter_Protocol_Lib.Requests;

public class TransactionRequest : IRequest
{
    public List<TransactionModel> Transactions { get; set; }

    public TransactionRequest()
    {
        Transactions = new List<TransactionModel>();
    }
}