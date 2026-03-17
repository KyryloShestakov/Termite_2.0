using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlockchainLib.Validator;
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
            _blockManager = new BlockManager();
        }

        public async Task<Response> PostTransactions(TerProtocol<object> request)
        {
            try
            {   
                TransactionRequest txRequest = (TransactionRequest)request.Payload.Data;
                
                if (txRequest is TransactionRequest tx)
                {
                    List<TransactionModel> transactions = txRequest.Transactions;
                    
                    IValidator validator = new TransactionValidator();
                    foreach (var transaction in transactions)
                    { 
                       Response result = await validator.Validate(transaction);
                       if (result.Status != "Success")
                       {
                           return _serverResponseService.GetResponse(false, "", transaction);
                       }
                    }
                    
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

        public async Task<Response> GetTransaction(TerProtocol<object> request)
        {
            try
            {
                TransactionRequest txRequest = (TransactionRequest)request.Payload.Data;
                Guid? guidId = txRequest.Id;
                string id = txRequest.Id.ToString();
                Logger.Log($"Id: {JsonSerializer.Serialize(txRequest)}", LogLevel.Warning, Source.Blockchain);
                
                List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
                List<BlockModel> blocks = models.Cast<BlockModel>().ToList();
                
                var transaction = blocks
                    .SelectMany(block => _blockManager.GetTransactionsFromBlock(block))
                    .FirstOrDefault(t => t.Id == id);
            
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
        public async Task<Response> GetTransactions(TerProtocol<object> request)
        {
            try
            {
                List<IModel> models =
                    await _dbProcessor.ProcessService<List<IModel>>(new TransactionBdService(new AppDbContext()),
                        CommandType.GetAll);
                List<TransactionModel> transactionModels = models.Cast<TransactionModel>().ToList();

                return _serverResponseService.GetResponse(true, "Transactions were found", transactionModels);
            }
            catch (Exception e)
            {
                Logger.Log($"{e.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

        public async Task<Response> UpdateTransactions(TerProtocol<object> request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> DeleteTransactions(TerProtocol<object> request)
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
