using System.Runtime.InteropServices.JavaScript;
using ModelsLib.BlockchainLib;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace BlockchainLib
{


    public class TransactionManager : ITransactionManager
    {
        private TransactionMemoryPool _memoryPool;
        private TransactionBdService _transactionBdService;
        private RedisService _redisService;

        public TransactionManager()
        {
            _memoryPool = new TransactionMemoryPool();
            _transactionBdService = new TransactionBdService(new AppDbContext());
            _redisService = new RedisService();
        }

        public async Task<Transaction> CreateTransaction(
            string senderAddress,
            string recipientAddress,
            decimal amount,
            decimal fee,
            string signature,
            Object data)
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
            Transaction transaction = new Transaction(senderAddress, recipientAddress, amount, fee, signature, data);
            string transactionType = data?.ToString();
            switch (transactionType)
            {
                case "Unconfirmed":
                    Logger.Log($"Transaction {transaction.Id} marked as unconfirmed. Adding to memory pool and Redis.",
                        LogLevel.Information, Source.Storage);
                    _memoryPool.AddTransaction(transaction);

                    string json = Transaction.Serialize(transaction);
                    await _redisService.SetStringAsync(transaction.Id, json);

                    Logger.Log($"Transaction {transaction.Id} serialized and saved to Redis.", LogLevel.Information,
                        Source.Storage);
                    break;

                case "Confirmed":
                    Logger.Log($"Transaction {transaction.Id} marked as confirmed. Adding to database.",
                        LogLevel.Information, Source.Storage);
                    TransactionModel transactionModel = Transaction.ToModel(transaction);
                    await _transactionBdService.AddTransactionAsync(transactionModel);

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


    }
}