using System.Security.Cryptography;
using System.Text;
using RRLib.Responses;
using BlockchainLib;
using Microsoft.Extensions.Logging;
using ModelsLib.BlockchainLib;
using Ter_Protocol_Lib;
using Utilities;
using LogLevel = Utilities.LogLevel;

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

        public async Task<Response> PostTransactions(TerProtocol<TransactionRequest> request)
        {
            try
            {
                List<TransactionModel> transactions = request.Payload.Data.Transactions;

                foreach (var transaction in transactions)
                {
                    Transaction newTransaction = await _transactionManager.CreateTransaction(transaction.Sender, transaction.Receiver, transaction.Amount, transaction.Fee, transaction.Signature, transaction.Data, transaction.PublicKey);
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

        public async Task<Response> GetTransactions(TerProtocol<TransactionRequest> request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> UpdateTransactions(TerProtocol<TransactionRequest> request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> DeleteTransactions(TerProtocol<TransactionRequest> request)
        {
            throw new NotImplementedException();
        }
        
        public string SignTransaction(TransactionModel transaction, string privateKey)
        {
            try
            {
                string key = privateKey.Substring(14, privateKey.Length - 14);
               // Logger.Log($"{key}", LogLevel.Information, Source.Blockchain);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    byte[] privateKeyBytes = Convert.FromBase64String(key);
            
                    rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

                    string dataToSign = $"{transaction.Sender}{transaction.Receiver}{transaction.Amount}{transaction.Fee}{transaction.Data}";
                    byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);

                    byte[] signatureBytes = rsa.SignData(dataBytes, new SHA256CryptoServiceProvider());
                    
                    return Convert.ToBase64String(signatureBytes);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error signing transaction: {e.Message}");
                throw;
            }
        }
    }
}
