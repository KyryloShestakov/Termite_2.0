using System.Security.Cryptography;
using System.Text;
using RRLib.Responses;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Ter_Protocol_Lib.Requests;
using Utilities;
using LogLevel = Utilities.LogLevel;

namespace BlockchainLib
{


    public class TransactionService
    {
        private TransactionManager _transactionManager;
        private ServerResponseService _serverResponseService;
        private readonly IDbProcessor _dbProcessor;
        private readonly BlockManager _blockManager;

        public TransactionService()
        {
            _transactionManager = new TransactionManager();
            _serverResponseService = new ServerResponseService();
            _dbProcessor = new DbProcessor();
        }

        public async Task<Response> PostTransactions(TerProtocol<object> request)
        {
            try
            {   
                TransactionRequest txRequest = (TransactionRequest)request.Payload.Data;
                
                if (txRequest is TransactionRequest tx)
                {
                    List<TransactionModel> transactions = txRequest.Transactions;

                    transactions.ForEach(transaction => transaction.Data = "Unconfirmed");

                    if (transactions == null || transactions.Count == 0)
                        return _serverResponseService.GetResponse(false, "No transactions found");
                    
                    await Task.WhenAll(transactions
                        .Select(transaction => _transactionManager.CreateTransaction(
                            transaction.Sender,
                            transaction.Receiver,
                            transaction.Amount,
                            transaction.Fee,
                            transaction.Signature,
                            transaction.Data,
                            transaction.PublicKey)));
                }
                else
                {
                    return _serverResponseService.GetResponse(false, "Invalid Transaction Data.");
                }
                
                Response response = _serverResponseService.GetResponse(true, "Transaction Created");
                
                return response;
            }
            catch (Exception e)
            {
                Logger.Log($"{e}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

        public async Task<Response> GetTransaction(TerProtocol<DataRequest<string>> request)
        {
            try
            {
                
                List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
                List<BlockModel> blocks = models.Cast<BlockModel>().ToList();
         
                var transaction = blocks
                    .SelectMany(block => _blockManager.GetTransactionsFromBlock(block))
                    .FirstOrDefault(t => t.Id == request.Payload.Data.Value);
            
                return transaction != null
                    ? _serverResponseService.GetResponse(true, "Transaction was found", transaction)
                    : _serverResponseService.GetResponse(false, "Transaction wasn't found", null);
            }
            catch (Exception e)
            {
                Logger.Log($"Error: {e.Message}", LogLevel.Error, Source.API);
                throw;
            }
        }
        public async Task<Response> GetTransactions(TerProtocol<DataRequest<string>> request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> UpdateTransactions(TerProtocol<DataRequest<string>> request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> DeleteTransactions(TerProtocol<DataRequest<string>> request)
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
