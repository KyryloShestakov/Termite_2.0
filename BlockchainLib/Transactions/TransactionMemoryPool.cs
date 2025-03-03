using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace BlockchainLib
{

    /// <summary>
    /// The TransactionMemoryPool class manages a pool of transactions that are waiting to be added to the blockchain.
    /// It stores transactions uniquely and provides the ability to prioritize transactions based on their fee.
    /// </summary>
    public class TransactionMemoryPool
    {
        /// <summary>
        /// A HashSet to store transaction IDs, ensuring no duplicates in the memory pool.
        /// </summary>
        private HashSet<string> transactionSet;

        /// <summary>
        /// A priority queue that stores transactions, sorted by fee in descending order.
        /// </summary>
        private PriorityQueue<Transaction, decimal> priorityQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionMemoryPool"/> class.
        /// </summary>
        public TransactionMemoryPool()
        {
            transactionSet = new HashSet<string>();
            priorityQueue = new PriorityQueue<Transaction, decimal>();
        }

        /// <summary>
        /// Adds a transaction to the memory pool. 
        /// If the transaction already exists, it will not be added again.
        /// </summary>
        /// <param name="transaction">The transaction to be added.</param>
        /// <returns>Returns true if the transaction is successfully added; otherwise, false.</returns>
        public bool AddTransaction(Transaction transaction)
        {
            // If the transaction already exists in the pool, it won't be added again.
            if (transactionSet.Contains(transaction.Id))
            {
                Logger.Log($"Transaction {transaction.Id} already exists in memory pool.", LogLevel.Information, Source.Blockchain);
                return false;
            }

            // Add the transaction to the set and enqueue it in the priority queue with the fee as priority.
            transactionSet.Add(transaction.Id);
            priorityQueue.Enqueue(transaction, -transaction.Fee); // Using a negative fee for max-heap behavior.
            Logger.Log($"Transaction {transaction.Id} added to memory pool.", LogLevel.Information, Source.Blockchain);
            return true;
        }

        /// <summary>
        /// Retrieves and removes the transaction with the highest fee from the memory pool.
        /// </summary>
        /// <returns>The transaction with the highest fee, or null if the pool is empty.</returns>
        public Transaction GetHighestFeeTransaction()
        {
            // If the pool is empty, return null.
            if (priorityQueue.Count == 0)
            {
                Logger.Log("Memory pool is empty.", LogLevel.Warning, Source.Blockchain);
                return null;
            }

            // Dequeue the transaction with the highest fee and remove it from the set.
            var transaction = priorityQueue.Dequeue();
            transactionSet.Remove(transaction.Id); // Remove from the set after dequeuing.
            Logger.Log($"Transaction {transaction.Id} removed from memory pool.", LogLevel.Information, Source.Blockchain);
            return transaction;
        }

        /// <summary>
        /// Checks if a transaction with the given ID exists in the memory pool.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to check.</param>
        /// <returns>True if the transaction exists in the pool, otherwise false.</returns>
        public bool ContainsTransaction(string transactionId)
        {
            return transactionSet.Contains(transactionId);
        }

        /// <summary>
        /// Gets the current number of transactions in the memory pool.
        /// </summary>
        /// <returns>The number of transactions in the memory pool.</returns>
        public int GetTransactionCount()
        {
            return priorityQueue.Count;
        }
        
        public void FillFromRedis(RedisService redisService)
        {
            var transactions = redisService.GetAllTransactionsAsync().Result;
            foreach (var transactionModel in transactions)
            {
                Transaction transaction = Transaction.ToEntity(transactionModel);
                AddTransaction(transaction);
            }
        }

        public async Task FillFromSqlLite()
        {
            IDbProcessor _dbProcessor = new DbProcessor();
            List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new TransactionBdService(new AppDbContext()), CommandType.GetAll);
            List<TransactionModel> transactions = models.Cast<TransactionModel>().ToList();
            
            foreach (var transactionModel in transactions)
            {
                Transaction transaction = Transaction.ToEntity(transactionModel);
                AddTransaction(transaction);
            }
        }
    }
}