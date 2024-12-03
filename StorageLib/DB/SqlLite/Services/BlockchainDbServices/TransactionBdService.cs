using Microsoft.EntityFrameworkCore;
using ModelsLib.BlockchainLib;
using Utilities;

namespace StorageLib.DB.SqlLite.Services.BlockchainDbServices
{
    public class TransactionBdService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor that initializes the service with the provided database context.
        /// </summary>
        /// <param name="context">Database context used for operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public TransactionBdService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Database context cannot be null.");
        }

        /// <summary>
        /// Adds a new transaction to the database.
        /// </summary>
        /// <param name="transaction">The transaction model to add.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddTransactionAsync(TransactionModel transaction)
        {
            if (transaction == null)
            {
                Logger.Log("Attempted to add a null transaction to the database.", LogLevel.Error, Source.Storage);
                throw new ArgumentNullException(nameof(transaction), "Transaction model cannot be null.");
            }

            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                Logger.Log($"Transaction added to DB | id:{transaction.Id}", LogLevel.Information, Source.Storage);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error adding transaction to DB: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a transaction by its ID.
        /// </summary>
        /// <param name="id">The ID of the transaction.</param>
        /// <returns>The transaction model or null if not found.</returns>
        public async Task<TransactionModel> GetTransactionByIdAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    Logger.Log($"Transaction not found | id:{id}", LogLevel.Warning, Source.Storage);
                }
                return transaction;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving transaction | id:{id} | Error: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all transactions from the database.
        /// </summary>
        /// <returns>A list of all transactions.</returns>
        public async Task<List<TransactionModel>> GetAllTransactionsAsync()
        {
            try
            {
                var transactions = await _context.Transactions.ToListAsync();
                Logger.Log($"Retrieved {transactions.Count} transactions from the database.", LogLevel.Information, Source.Storage);
                return transactions;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving all transactions: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Deletes a transaction by its ID.
        /// </summary>
        /// <param name="id">The ID of the transaction to delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task DeleteTransactionAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    Logger.Log($"Transaction not found for deletion | id:{id}", LogLevel.Warning, Source.Storage);
                    return;
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                Logger.Log($"Transaction deleted | id:{id}", LogLevel.Information, Source.Storage);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deleting transaction | id:{id} | Error: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing transaction in the database.
        /// </summary>
        /// <param name="transaction">The updated transaction model.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task UpdateTransactionAsync(TransactionModel transaction)
        {
            if (transaction == null)
            {
                Logger.Log("Attempted to update a null transaction.", LogLevel.Error, Source.Storage);
                throw new ArgumentNullException(nameof(transaction), "Transaction model cannot be null.");
            }

            try
            {
                var existingTransaction = await _context.Transactions.FindAsync(transaction.Id);
                if (existingTransaction == null)
                {
                    Logger.Log($"Transaction not found for update | id:{transaction.Id}", LogLevel.Warning, Source.Storage);
                    return;
                }

                existingTransaction.Data = transaction.Data;
                existingTransaction.Signature = transaction.Signature;
                existingTransaction.Timestamp = transaction.Timestamp;

                await _context.SaveChangesAsync();
                Logger.Log($"Transaction updated | id:{transaction.Id}", LogLevel.Information, Source.Storage);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating transaction | id:{transaction.Id} | Error: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }
    }
}
