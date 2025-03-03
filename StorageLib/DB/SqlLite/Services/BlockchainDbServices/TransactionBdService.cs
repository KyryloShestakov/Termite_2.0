using DataLib.DB.SqlLite.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelsLib;
using ModelsLib.BlockchainLib;
using Utilities;
using LogLevel = Utilities.LogLevel;

namespace StorageLib.DB.SqlLite.Services.BlockchainDbServices
{
    public class TransactionBdService : IDbProvider
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor that initializes the service with the provided database context.
        /// </summary>
        /// <param name="context">Database context used for operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public TransactionBdService(AppDbContext context)
        {
            _context = new AppDbContext();
            _context = context ?? throw new ArgumentNullException(nameof(context), "Database context cannot be null.");
        }

        public async Task<bool> Add(IModel model)
        {
            TransactionModel transaction = model as TransactionModel;
            if (transaction == null)
            {
                Logger.Log("Attempted to add a null transaction to the database.", LogLevel.Error, Source.Storage);
                throw new ArgumentNullException(nameof(transaction), "Transaction model cannot be null.");
            }

            try
            {
                _context.Transactions.Add(transaction);
                Logger.Log($"Public key is {transaction.PublicKey}.", LogLevel.Information, Source.Storage);
                await _context.SaveChangesAsync();
                Logger.Log($"Transaction added to DB | id:{transaction.Id}", LogLevel.Information, Source.Storage);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error adding transaction to DB: {ex.Message}", LogLevel.Error, Source.Storage);
                if (ex.InnerException != null)
                {
                    Logger.Log($"Inner Exception: {ex.InnerException.Message}", LogLevel.Error, Source.Storage);
                    Logger.Log($"Stack Trace: {ex.InnerException.StackTrace}", LogLevel.Error, Source.Storage);
                }
                return false;
            }
        }

        public async Task<IModel> Get(string id)
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

        public async Task<List<IModel>> GetAll()
        {
            try
            {
                var transactions = await _context.Transactions.ToListAsync();
                Logger.Log($"Retrieved {transactions.Count} transactions from the database.", LogLevel.Information, Source.Storage);
                return new List<IModel>(transactions);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving all transactions: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    Logger.Log($"Transaction not found for deletion | id:{id}", LogLevel.Warning, Source.Storage);
                    return false;
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                Logger.Log($"Transaction deleted | id:{id}", LogLevel.Information, Source.Storage);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deleting transaction | id:{id} | Error: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        public async Task<bool> Update(string id, IModel model)
        {
            TransactionModel transaction = model as TransactionModel;
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
                    return false;
                }

                existingTransaction.Data = transaction.Data;
                existingTransaction.Signature = transaction.Signature;
                existingTransaction.Timestamp = transaction.Timestamp;

                await _context.SaveChangesAsync();
                Logger.Log($"Transaction updated | id:{transaction.Id}", LogLevel.Information, Source.Storage);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating transaction | id:{transaction.Id} | Error: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        public async Task<bool> Exists(string id)
        {
            return true;
        }
    }
}
