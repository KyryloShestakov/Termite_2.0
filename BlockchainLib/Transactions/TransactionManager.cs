using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using Newtonsoft.Json;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace BlockchainLib
{


    public class TransactionManager : ITransactionManager
    {
        private TransactionMemoryPool _memoryPool;
        private readonly IDbProcessor _dbProcessor;
        private readonly AppDbContext _appDbContext;
        public TransactionManager()
        {
            _memoryPool = new TransactionMemoryPool();
            _appDbContext = new AppDbContext();
            _dbProcessor = new DbProcessor();
        }

        public async Task<Transaction> CreateTransaction(
            string senderAddress,
            string recipientAddress,
            decimal amount,
            decimal fee,
            string signature,
            Object data,
            string pubKey)
        {
            // Validation of input parameters
            if (string.IsNullOrWhiteSpace(senderAddress))
            {
                Logger.Log("Sender address is null or empty", LogLevel.Error, Source.Blockchain);
                throw new ArgumentException("Sender address cannot be null or empty", nameof(senderAddress));
            }

            if (string.IsNullOrWhiteSpace(recipientAddress))
            {
                Logger.Log("Recipient address is null or empty", LogLevel.Error, Source.Blockchain);
                throw new ArgumentException("Recipient address cannot be null or empty", nameof(recipientAddress));
            }

            if (amount <= 0)
            {
                Logger.Log("Amount is zero or negative", LogLevel.Error, Source.Blockchain);
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            }

            if (fee < 0)
            {
                Logger.Log("Fee is negative", LogLevel.Error, Source.Blockchain);
                throw new ArgumentException("Fee cannot be negative", nameof(fee));
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                Logger.Log("Signature is null or empty", LogLevel.Error, Source.Blockchain);
                throw new ArgumentException("Signature cannot be null or empty", nameof(signature));
            }

            Logger.Log(
                $"Creating transaction: Sender={senderAddress}, Receiver={recipientAddress}, Amount={amount}, Fee={fee}",
                LogLevel.Information, Source.Blockchain);

            // Transaction creation
            Transaction transaction = new Transaction(senderAddress, recipientAddress, amount, fee, signature, data, pubKey);
            string transactionType = data?.ToString();
            switch (transactionType)
            {
                case "Unconfirmed":
                    Logger.Log($"Transaction {transaction.Id} marked as unconfirmed. Adding to memory pool and DB.",
                        LogLevel.Information, Source.Storage);
                    _memoryPool.AddTransaction(transaction);

                    // string json = Transaction.Serialize(transaction);
                    // await _redisService.SetStringAsync(transaction.Id, json);
                    TransactionModel transactionmodel = Transaction.ToModel(transaction);                 
                    bool isCreated = await _dbProcessor.ProcessService<bool>(new TransactionBdService(new AppDbContext()), CommandType.Add, new DbData(transactionmodel));
                    if (!isCreated)
                    {
                        Logger.Log($"Transaction {transaction.Id} was not created", LogLevel.Warning, Source.Blockchain);
                    }
                   // Logger.Log($"Transaction {transaction.Id} serialized and saved to Redis.", LogLevel.Information,Source.Storage);
                    break;

                case "Confirmed":
                    Logger.Log($"Transaction {transaction.Id} marked as confirmed. Adding to database.",
                        LogLevel.Information, Source.Storage);
                    TransactionModel transactionModel = Transaction.ToModel(transaction);
                    await _dbProcessor.ProcessService<bool>(new TransactionBdService(_appDbContext), CommandType.Add, new DbData(transactionModel));

                    Logger.Log($"Transaction {transaction.Id} saved to the database.", LogLevel.Information,
                        Source.Storage);
                    break;

                default:
                    Logger.Log($"Invalid transaction type: {data}", LogLevel.Error, Source.Blockchain);
                    throw new ArgumentException($"Invalid transaction type: {data}", nameof(data));
            }

            Logger.Log($"Transaction {transaction.Id} created successfully.", LogLevel.Information, Source.Blockchain);
            return transaction;
        }

        public bool ValidateTransaction(Transaction transactionModel)
        {
            return true;
        }

        
        public List<Transaction> GetTransactionsFromBlock(BlockModel blockModel)
        {
            var transactions = new List<Transaction>();
            
                if (!string.IsNullOrEmpty(blockModel.Transactions))
                {
                    try
                    {
                        transactions = JsonConvert.DeserializeObject<List<Transaction>>(blockModel.Transactions);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error deserializing transactions: {ex.Message}", LogLevel.Error, Source.App);
                    }
                }

                return transactions;
        }
        
        public TransactionModel SignTransaction(TransactionModel transaction, string privateKey)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));

            string transactionData = JsonConvert.SerializeObject(new
            {
                transaction.Sender,
                transaction.Receiver,
                transaction.Amount,
                transaction.Timestamp,
                transaction.Fee,
                transaction.Data,
                transaction.Contract
            });

            byte[] transactionHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(transactionData));

            RSA privateKeyRsa = GetPrivateKeyFromString(privateKey);
            byte[] signature = privateKeyRsa.SignData(transactionHash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            transaction.Signature = Convert.ToBase64String(signature);

            return transaction;
        }
        public Transaction SignTransaction(Transaction transaction, string privateKey)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));

            TransactionModel transactionModel = Transaction.ToModel(transaction);
            string transactionData = JsonConvert.SerializeObject(new
            {
                transactionModel.Sender,
                transactionModel.Receiver,
                transactionModel.Amount,
                transactionModel.Timestamp,
                transactionModel.Fee,
                transactionModel.Data,
                transactionModel.Contract
            });

            byte[] transactionHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(transactionData));

            RSA privateKeyRsa = GetPrivateKeyFromString(privateKey);
            byte[] signature = privateKeyRsa.SignData(transactionHash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            transaction.Signature = Convert.ToBase64String(signature);

            return transaction;
        }
            
        private RSA GetPrivateKeyFromString(string privateKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
                return rsa;
            }
        }

        public async Task<Transaction> CreateAwardedTransaction(Block block)
        {
            int blockHeight = 210000;
            decimal amount = CalculateBlockReward(blockHeight, block.Transactions);

            MyPrivatePeerInfoModel myInfo = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));
            
            Transaction transaction = new Transaction()
            {
                Id = Guid.NewGuid().ToString(),
                Amount = amount,
                BlockId = block.Id,
                Fee = 0,
                PublicKey = myInfo.PublicKey,
                Sender = "Awarded transaction",
                Receiver = myInfo.AddressWallet,
                Data = new string("Awarded transaction data"),
                Signature = string.Empty,
            };
            
           // Transaction transactionModel = SignTransaction(transaction, myInfo.PrivateKey);
            
            return transaction;
        }
        private const decimal InitialReward = 50m;
        private const int HalvingInterval = 210000;
        public static decimal CalculateBlockReward(int blockHeight, List<Transaction> transactions)
        {
            int halvings = blockHeight / HalvingInterval;
        
            decimal blockReward = InitialReward / (decimal)Math.Pow(2, halvings);
            
            decimal totalFees = transactions.Sum(x => x.Fee);
            
            return blockReward + totalFees;
        }

    }
    
}