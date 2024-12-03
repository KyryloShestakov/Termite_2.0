using ModelsLib.BlockchainLib;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;

namespace BlockchainLib
{
    public class BlockService
    {
        private BlockManager _blockManager;
        private ServerResponseService _serverResponseService;
        private BlocksBdService _blocksBdService;
        private Blockchain _blockchain;

        public BlockService()
        {
            // Initialize dependencies
            _blockManager = new BlockManager();
            _serverResponseService = new ServerResponseService();
            _blocksBdService = new BlocksBdService(new AppDbContext());
        }

        /// <summary>
        /// Adds a new block to the blockchain and database.
        /// </summary>
        public async Task<Response> PostBlocks(BlockRequest request)
        {
            if (request == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");

            BlockModel blockModel = request.GetBlock();

            if (blockModel == null)
                return _serverResponseService.GetResponse(false, "Invalid block data.");

            try
            {
                // Add the block to the database
                await _blocksBdService.AddBlockAsync(blockModel);

                // Convert the model to a blockchain block and add it to the blockchain
                Block block = new Block();
                block = Block.FromBlockModel(blockModel);
                await _blockchain.AddBlockAsync(block);

                // Return a success response
                return _serverResponseService.GetResponse(true, "Block created successfully.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur
                return _serverResponseService.GetResponse(false, $"Error adding block: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all blocks from the database.
        /// </summary>
        public async Task<Response> GetBlocks(BlockRequest request)
        {
            try
            {
                // Fetch all blocks from the database
                List<BlockModel> blocks = await _blocksBdService.GetAllBlocksAsync();

                // Return the blocks in the response
                return _serverResponseService.GetResponse(true, "Blocks retrieved successfully.", blocks);
            }
            catch (Exception ex)
            {
                return _serverResponseService.GetResponse(false, $"Error retrieving blocks: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific block by its index.
        /// </summary>
        public async Task<Response> GetBlockById(BlockRequest request)
        {
            if (request == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");

            BlockModel requestedBlock = request.GetBlock();

            if (requestedBlock == null)
                return _serverResponseService.GetResponse(false, "Invalid block data.");

            try
            {
                // Fetch the block by index
                BlockModel blockModel = await _blocksBdService.GetBlockAsync(requestedBlock.Index);

                if (blockModel == null)
                    return _serverResponseService.GetResponse(false, "Block not found.");

                return _serverResponseService.GetResponse(true, "Block retrieved successfully.", blockModel);
            }
            catch (Exception ex)
            {
                return _serverResponseService.GetResponse(false, $"Error retrieving block: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing block in the database.
        /// </summary>
        public async Task<Response> UpdateBlocks(BlockRequest request)
        {
            if (request == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");

            BlockModel blockModelToUpdate = request.GetBlock();

            if (blockModelToUpdate == null)
                return _serverResponseService.GetResponse(false, "Invalid block data.");

            try
            {
                // Check if the block exists
                BlockModel existingBlock = await _blocksBdService.GetBlockAsync(blockModelToUpdate.Index);
                if (existingBlock == null)
                    return _serverResponseService.GetResponse(false, "Block not found.");

                // Update the block in the database
                await _blocksBdService.UpdateBlockAsync(blockModelToUpdate);

                return _serverResponseService.GetResponse(true, "Block updated successfully.");
            }
            catch (Exception ex)
            {
                return _serverResponseService.GetResponse(false, $"Error updating block: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a block from the database.
        /// </summary>
        public async Task<Response> DeleteBlocks(BlockRequest request)
        {
            if (request == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");

            BlockModel blockModelToDelete = request.GetBlock();

            if (blockModelToDelete == null)
                return _serverResponseService.GetResponse(false, "Invalid block data.");

            try
            {
                // Check if the block exists
                BlockModel existingBlock = await _blocksBdService.GetBlockAsync(blockModelToDelete.Index);
                if (existingBlock == null)
                    return _serverResponseService.GetResponse(false, "Block not found.");

                // Delete the block from the database
                await _blocksBdService.DeleteBlockAsync(blockModelToDelete.Index);

                return _serverResponseService.GetResponse(true, "Block deleted successfully.");
            }
            catch (Exception ex)
            {
                return _serverResponseService.GetResponse(false, $"Error deleting block: {ex.Message}");
            }
        }
    }
}
