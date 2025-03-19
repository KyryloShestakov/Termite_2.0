using BlockchainLib.Validator;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using RRLib.Responses;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace BlockchainLib.Blocks
{
    public class BlockService
    {
        private BlockManager _blockManager;
        private readonly ServerResponseService _serverResponseService;
        private readonly Blockchain _blockchain;
        private readonly IDbProcessor _dbProcessor;
        private readonly AppDbContext _appDbContext;

        public BlockService()
        {
            // Initialize dependencies
            _blockchain = new Blockchain();
            _blockManager = new BlockManager();
            _serverResponseService = new ServerResponseService();
            _appDbContext = new AppDbContext();
            _dbProcessor = new DbProcessor();
        }

        /// <summary>
        /// Adds a new block to the blockchain and database.
        /// </summary>
        public async Task<Response> PostBlocks(TerProtocol<object> request)
        {
            if (request == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");
            
            BlockRequest blockRequest = (BlockRequest)request.Payload.Data;

            if (blockRequest == null)
                return _serverResponseService.GetResponse(false, "Invalid block data.");
            
            try
            {
                // Validate block
                BlockchainModel blockchainModel = new BlockchainModel();
                blockchainModel.Blocks = blockRequest.Blocks;
                
                IValidator validator = new BlockChainValidator();
                Response isValid = await validator.Validate(blockchainModel);
                if (isValid.Status != "Success") { return _serverResponseService.GetResponse(false, "Invalid block data."); }
                
                foreach (BlockModel block in blockchainModel.Blocks)
                {
                    bool existsInDb = await _dbProcessor.ProcessService<bool>(new BlocksBdService(new AppDbContext()), CommandType.Exists, new DbData(null, block.Id));
                    Logger.Log($"Block {block.Id} existed in bd", LogLevel.Warning, Source.Blockchain);
                    if (existsInDb) continue;
                    await _dbProcessor.ProcessService<bool>(new BlocksBdService(new AppDbContext()), CommandType.Add, new DbData(block));
                    Block blockForModel = new Block();
                    blockForModel = Block.FromBlockModel(block);
                    await _blockchain.AddBlockAsync(blockForModel);
                }
                

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
        public async Task<Response> GetBlocks(TerProtocol<BlockRequest> request)
        {
            try
            {
                // Fetch all blocks from the database
                List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
                List<BlockModel> blocks = models.Cast<BlockModel>().ToList();
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
        public async Task<Response> GetBlockById(TerProtocol<DataRequest<string>> blockRequest)
        {
            if (blockRequest == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");

            string index = blockRequest.Payload.Data.Value;

            if (index == null)
                return _serverResponseService.GetResponse(false, "Invalid index.");

            try
            {
                // Fetch the block by index
                IModel model = await _dbProcessor.ProcessService<IModel>(new BlocksBdService(_appDbContext), CommandType.Get, new DbData(null, index));
                BlockModel blockModel = model as BlockModel ?? throw new InvalidOperationException();
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
        public async Task<Response> UpdateBlocks(TerProtocol<BlockRequest> request)
        {
            if (request == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");

            BlockModel blockModelToUpdate = request.Payload.Data.Blocks.First();

            if (blockModelToUpdate == null)
                return _serverResponseService.GetResponse(false, "Invalid block data.");

            try
            {
                // Check if the block exists
                bool result = await _dbProcessor.ProcessService<bool>(new BlocksBdService(_appDbContext), CommandType.Exists, new DbData(null, blockModelToUpdate.Index.ToString()));
                if (result == false)
                    return _serverResponseService.GetResponse(false, "Block not found.");

                // Update the block in the database
                await _dbProcessor.ProcessService<bool>(new BlocksBdService(_appDbContext), CommandType.Update, new DbData(blockModelToUpdate, blockModelToUpdate.Index.ToString()));

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
        public async Task<Response> DeleteBlock(TerProtocol<BlockRequest> request)
        {
            if (request == null)
                return _serverResponseService.GetResponse(false, "Invalid request: request cannot be null.");

            BlockModel blockModelToDelete = request.Payload.Data.Blocks.First();

            if (blockModelToDelete == null)
                return _serverResponseService.GetResponse(false, "Invalid block data.");

            try
            {
                // Check if the block exists
                bool result = await _dbProcessor.ProcessService<bool>(new BlocksBdService(_appDbContext), CommandType.Exists, new DbData(null, blockModelToDelete.Index.ToString()));
                if (result is false)
                    return _serverResponseService.GetResponse(false, "Block not found.");

                // Delete the block from the database
                await _dbProcessor.ProcessService<bool>(new BlocksBdService(_appDbContext), CommandType.Delete, new DbData(null, blockModelToDelete.Id));

                return _serverResponseService.GetResponse(true, "Block deleted successfully.");
            }
            catch (Exception ex)
            {
                return _serverResponseService.GetResponse(false, $"Error deleting block: {ex.Message}");
            }
        }
    }
}
