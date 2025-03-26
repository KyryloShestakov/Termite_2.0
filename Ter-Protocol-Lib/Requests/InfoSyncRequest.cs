using ModelsLib.BlockchainLib;

namespace Ter_Protocol_Lib.Requests;

public class InfoSyncRequest : IRequest
{
    public int BlocksCount { get; set; }
    public DateTime TimeOfLastBlock { get; set; }
    public string LastBlockHash { get; set; }
    public int CountOfTransactions { get; set; }
    public List<string> TransactionIds { get; set; }

    public InfoSyncRequest()
    {
        TransactionIds = new List<string>();
    }

}