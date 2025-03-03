using ModelsLib;
using ModelsLib.BlockchainLib;
using Utilities;

namespace BlockchainLib.Validator 
{
    public class BlockChainValidator : IValidator
    {
        private readonly int MaxBlockSize = 10000;
        
        public async Task<bool> Validate(IModel model)
        {
            try
            {
                if (model is not BlockchainModel blockchain || blockchain.Blocks == null || !blockchain.Blocks.Any())
                {
                    Logger.Log("Blockchain validation failed: Invalid or empty blockchain model.", LogLevel.Error, Source.Validator);
                    return false;
                }

                Logger.Log($"Starting validation of {blockchain.Blocks.Count} blocks", LogLevel.Information, Source.Validator);

                for (int i = 1; i < blockchain.Blocks.Count; i++)
                {
                    var previousBlock = blockchain.Blocks[i - 1];
                    var currentBlock = blockchain.Blocks[i];

                    if (!ValidateBlock(currentBlock))
                    {
                        Logger.Log($"Block validation failed at index {currentBlock.Index}", LogLevel.Error, Source.Validator);
                        return false;
                    }

                    if (!ValidateBlockLink(previousBlock, currentBlock))
                    {
                        Logger.Log($"Invalid link between block {previousBlock.Index} and {currentBlock.Index}", LogLevel.Error, Source.Validator);
                        return false;
                    }
                }

                if (!ValidateGenesisBlock(blockchain.Blocks.First()))
                {
                    Logger.Log("Genesis block validation failed.", LogLevel.Error, Source.Validator);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Blockchain validation error: {ex.Message}", LogLevel.Error, Source.Validator);
                return false;
            }
        }

        private bool ValidateBlock(BlockModel block)
        {
            try
            {
                if (block == null)
                {
                    Logger.Log("Block validation failed: Block is null.", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(block.Id))
                {
                    Logger.Log($"Block validation failed: Block ID is missing (Index: {block.Index}).", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (block.Size > MaxBlockSize)
                {
                    Logger.Log($"Block validation failed: Block size {block.Size} exceeds the limit {MaxBlockSize} (Index: {block.Index}).", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (!ValidateHash(block.Hash, block))
                {
                    Logger.Log($"Block validation failed: Hash mismatch (Index: {block.Index}).", LogLevel.Error, Source.Validator);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in ValidateBlock: {ex.Message}", LogLevel.Error, Source.Validator);
                return false;
            }
        }

        private bool ValidateHash(string blockHash, BlockModel blockModel)
        {
            try
            {
                Block block = Block.FromBlockModel(blockModel);
                string generatedHash = block.GenerateHash();

                Logger.Log($"Calculated hash: {generatedHash}, received hash: {blockHash}", LogLevel.Information, Source.Validator);

                if (blockHash == generatedHash)
                {
                    Logger.Log("Block hash is valid.", LogLevel.Information, Source.Validator);
                    return true;
                }
                else
                {
                    Logger.Log("Block hash is invalid!", LogLevel.Error, Source.Validator);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in ValidateHash: {ex.Message}", LogLevel.Error, Source.Validator);
                return false;
            }
        }

        private bool ValidateBlockLink(BlockModel previousBlock, BlockModel currentBlock)
        {
            try
            {
                if (previousBlock == null || currentBlock == null)
                {
                    Logger.Log("Block link validation failed: One of the blocks is null.", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (currentBlock.Index != previousBlock.Index + 1)
                {
                    Logger.Log($"Block link validation failed: Invalid index sequence ({previousBlock.Index} -> {currentBlock.Index}).", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    Logger.Log($"Block link validation failed: Previous hash mismatch at index {currentBlock.Index}.", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (currentBlock.Timestamp <= previousBlock.Timestamp || currentBlock.Timestamp > DateTime.UtcNow.AddMinutes(2))
                {
                    Logger.Log($"Block link validation failed: Invalid timestamp in block {currentBlock.Index}.", LogLevel.Error, Source.Validator);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in ValidateBlockLink: {ex.Message}", LogLevel.Error, Source.Validator);
                return false;
            }
        }

        private bool ValidateGenesisBlock(BlockModel genesisBlock)
        {
            try
            {
                if (genesisBlock == null)
                {
                    Logger.Log("Genesis block validation failed: Block is null.", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (!ValidateBlock(genesisBlock))
                {
                    Logger.Log("Genesis block validation failed: General block validation failed.", LogLevel.Error, Source.Validator);
                    return false;
                }

                if (genesisBlock.Index != 0)
                {
                    Logger.Log("Genesis block validation failed: Index is not 0.", LogLevel.Error, Source.Validator);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in ValidateGenesisBlock: {ex.Message}", LogLevel.Error, Source.Validator);
                return false;
            }
        }
    }
}
