using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Responses;
using BlockchainLib;
using ModelsLib.BlockchainLib;

namespace BlockchainLib
{


    public class TransactionService
    {
        private TransactionManager _transactionManager;
        private ServerResponseService _serverResponseService;

        public TransactionService()
        {
            _transactionManager = new TransactionManager();
            _serverResponseService = new ServerResponseService();
        }

        public async Task<Response> PostTransactions(Request request)
        {
            try
            {
                TransactionRequest transactionRequest = request as TransactionRequest;
                List<TransactionModel> transactions = transactionRequest.GetTransactions();

                foreach (var transaction in transactions)
                {
                    Transaction newTransaction = await _transactionManager.CreateTransaction(transaction.Sender, transaction.Receiver, transaction.Amount, transaction.Fee, transaction.Signature, transaction.Data);
                }
                
                Response response = _serverResponseService.GetResponse(true, "Transaction Created");

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Response> GetTransactions(Request request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> UpdateTransactions(Request request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> DeleteTransactions(Request request)
        {
            throw new NotImplementedException();
        }
    }
}
