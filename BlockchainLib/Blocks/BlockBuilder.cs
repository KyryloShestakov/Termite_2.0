using Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StorageLib.DB.Redis;

namespace BlockchainLib.Blocks
{
    public class BlockBuilder
    {
        private readonly TransactionMemoryPool _transactionMemoryPool;
        private const int MaxWaitTimeInSeconds = 60;
        private DateTime lastTransactionTime;
        private const int MaxTransactionLimit = 1;
        private Blockchain _blockChain;
        private MerkleTree _merkleTree;
        private BlockManager _blockManager;
        public int CurrentDifficulty { get; set; } = 1;

        // Constructor to initialize dependencies
        public BlockBuilder()
        {
            _transactionMemoryPool = new TransactionMemoryPool();
            _blockChain = new Blockchain();
            _blockManager = new BlockManager();
            _merkleTree = new MerkleTree();
            lastTransactionTime = DateTime.Now;
        }

        public async Task<Block> StartBuilding()
        {
            try
            {
                if (await TryBuildBlock())
                {
                    if (await _blockChain.GetCountAsync() == 0)
                    {
                        Logger.Log("Blockchain is empty. Starting to create the genesis block.");
                        Block genesisBlock = await CreateGenesisBlock();
                        Logger.Log($"Genesis block created: {genesisBlock.Id}", LogLevel.Information, Source.Blockchain);
                        return genesisBlock;
                    }
                    
                    Block block = await BuildBlockAsync();
                    Logger.Log($"Block created: {block.Id}",LogLevel.Information, Source.Blockchain);
                    return block;
                }
                else
                {
                    Logger.Log("Conditions for building a block not met.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during block building process: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> TryBuildBlock()
        {
            try
            {
                RedisService redisService = new RedisService();
                _transactionMemoryPool.FillFromRedis(redisService);
                 int num =_transactionMemoryPool.GetTransactionCount();
                Logger.Log(num.ToString());
                if (_transactionMemoryPool.GetTransactionCount() >= MaxTransactionLimit || (DateTime.UtcNow - lastTransactionTime).TotalSeconds > MaxWaitTimeInSeconds)
                {
                    lastTransactionTime = DateTime.UtcNow;
                    return true;
                }

                Logger.Log("Waiting for more transactions to build the block.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in TryBuildBlock: {ex.Message}");
                return false;
            }
        }

        private async Task<Block> BuildBlockAsync()
        {
            try
            {
                string previousBlockHash = await _blockChain.GetLastBlockHashAsync();

                int index = await _blockChain.GetCountAsync() + 1;

                Block block = new Block(index, new List<Transaction>(), previousBlockHash, string.Empty, CurrentDifficulty, string.Empty, string.Empty);

                while (block.Transactions.Count < MaxTransactionLimit && _transactionMemoryPool.GetTransactionCount() > 0)
                {
                    var tx = _transactionMemoryPool.GetHighestFeeTransaction();
                    block.Transactions.Add(tx);
                }

                block.MerkleRoot = _merkleTree.CalculateMerkleRoot(block.Transactions);
                Logger.Log("Merkle root calculated.");

                block.Nonce = MineBlock(block);
                Logger.Log("Block mined.");

                block.Hash = block.GenerateHash();
                Logger.Log($"Block hash generated: {block.Hash}");

                block.Signature = SignBlock(block);
                Logger.Log("Block signature created.");

                block.Size = block.CalculateSize();
                Logger.Log($"Block size: {block.Size} bytes.");

                return block;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in BuildBlockAsync: {ex.Message}");
                throw;
            }
        }

        private string MineBlock(Block block)
        {
            try
            {
                string nonce = string.Empty;
                string hash;
                int difficulty = block.Difficulty;

                do
                {
                    nonce = GenerateNonce();
                    block.Nonce = nonce;
                    hash = block.GenerateHash();
                }
                while (!hash.StartsWith(new string('0', difficulty)));

                return nonce;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in MineBlock: {ex.Message}");
                throw;
            }
        }

        private string GenerateNonce()
        {
            try
            {
                Random random = new Random();
                int randomValue = random.Next(1, int.MaxValue);

                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

                string nonce = $"{timestamp}-{randomValue}";

                return nonce;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GenerateNonce: {ex.Message}");
                throw;
            }
        }

        private string SignBlock(Block block)
        {
            try
            {
                return _blockManager.SignBlock(block.GenerateHash());
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in SignBlock: {ex.Message}");
                throw;
            }
        }

        private async Task<Block> CreateGenesisBlock()
        {
            try
            {
                Logger.Log("Creating genesis block");
                string id = Guid.NewGuid().ToString();
                int index = 1;
                string previousHash = "0";
                string merkleRoot = null;
                int difficulty = 2;
                string nonce = "randomNonce123";
                string signature = "blockSignature";

                Block genesisBlock = new Block(index, null, previousHash, merkleRoot, difficulty, nonce, signature);
                
                Logger.Log("Genesis block created successfully.");
                return genesisBlock;
            }
            catch (Exception ex)
            {
                // Log error
                Logger.Log($"Error while creating genesis block: {ex.Message}");
                throw;  // Propagate the error
            }
        }
    }
}
