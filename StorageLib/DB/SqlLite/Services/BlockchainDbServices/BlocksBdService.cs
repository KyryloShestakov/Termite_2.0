using Microsoft.EntityFrameworkCore;
using ModelsLib.BlockchainLib;
using Utilities;

namespace StorageLib.DB.SqlLite.Services.BlockchainDbServices
{
    public class BlocksBdService
    {
        private readonly AppDbContext _context;

        public BlocksBdService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Database context cannot be null.");
        }

        /// <summary>
        /// Adds a block to the database.
        /// </summary>
        /// <param name="blockModel">The block model to add.</param>
        public async Task AddBlockAsync(BlockModel blockModel)
        {
            if (blockModel == null)
            {
                Logger.Log("Attempted to add a null block to the database.", LogLevel.Error, Source.Storage);
                throw new ArgumentNullException(nameof(blockModel), "Block model cannot be null.");
            }

            try
            {
                
                _context.Blocks.Add(blockModel);
                await _context.SaveChangesAsync();
                Logger.Log($"Block added to DB | id:{blockModel.Index}", LogLevel.Information, Source.Storage);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error adding block to DB: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Deletes a block by its ID.
        /// </summary>
        /// <param name="id">The ID of the block to delete.</param>
        public async Task DeleteBlockAsync(string id)
        {
            try
            {
                var block = await _context.Blocks.FindAsync(id);
                if (block != null)
                {
                    _context.Blocks.Remove(block);
                    await _context.SaveChangesAsync();
                    Logger.Log($"Block deleted from DB | id:{id}", LogLevel.Information, Source.Storage);
                }
                else
                {
                    Logger.Log($"Attempted to delete a non-existent block | id:{id}", LogLevel.Warning, Source.Storage);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deleting block from DB: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a block by its ID.
        /// </summary>
        /// <param name="id">The ID of the block to retrieve.</param>
        /// <returns>The block model, or null if not found.</returns>
        public async Task<BlockModel> GetBlockAsync(int id)
        {
            try
            {
                var block = await _context.Blocks.FindAsync(id);
                if (block == null)
                {
                    Logger.Log($"Block not found in DB | id:{id}", LogLevel.Warning, Source.Storage);
                }
                return block;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving block from DB: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all blocks from the database.
        /// </summary>
        /// <returns>A list of all block models.</returns>
        public async Task<List<BlockModel>> GetAllBlocksAsync()
        {
            try
            {
                var blocks = await _context.Blocks.ToListAsync();
                Logger.Log($"Retrieved {blocks.Count} blocks from DB.", LogLevel.Information, Source.Storage);
                return blocks;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving all blocks from DB: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing block in the database.
        /// </summary>
        /// <param name="blockModel">The block model with updated data.</param>
        public async Task UpdateBlockAsync(BlockModel blockModel)
        {
            if (blockModel == null)
            {
                Logger.Log("Attempted to update a null block in the database.", LogLevel.Error, Source.Storage);
                throw new ArgumentNullException(nameof(blockModel), "Block model cannot be null.");
            }

            try
            {
                var existingBlock = await _context.Blocks.FindAsync(blockModel.Index);
                if (existingBlock != null)
                {
                    // Update block properties
                    existingBlock.Timestamp = blockModel.Timestamp;
                    existingBlock.MerkleRoot = blockModel.MerkleRoot;
                    existingBlock.PreviousHash = blockModel.PreviousHash;
                    existingBlock.Hash = blockModel.Hash;
                    existingBlock.Difficulty = blockModel.Difficulty;
                    existingBlock.Nonce = blockModel.Nonce;
                    existingBlock.Signature = blockModel.Signature;
                    existingBlock.Size = blockModel.Size;
                    existingBlock.Transactions = blockModel.Transactions;

                    await _context.SaveChangesAsync();
                    Logger.Log($"Block updated in DB | id:{blockModel.Index}", LogLevel.Information, Source.Storage);
                }
                else
                {
                    Logger.Log($"Attempted to update a non-existent block | id:{blockModel.Index}", LogLevel.Warning, Source.Storage);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating block in DB: {ex.Message}", LogLevel.Error, Source.Storage);
                throw;
            }
        }
    }
}
