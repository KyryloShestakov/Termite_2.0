using Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;

namespace BlockchainLib.Blocks
{
    public class BlockBuilder
    {
        private readonly TransactionMemoryPool _transactionMemoryPool;
        private const int MaxWaitTimeInSeconds = 30000;
        private DateTime lastTransactionTime;
        private const int MaxTransactionLimit = 1;
        private Blockchain _blockChain;
        private MerkleTree _merkleTree;
        private BlockManager _blockManager;
        private readonly IDbProcessor _dbProcessor;
        public int CurrentDifficulty { get; set; } = 2;

        // Constructor to initialize dependencies
        public BlockBuilder()
        {
            _transactionMemoryPool = new TransactionMemoryPool();
            _blockChain = new Blockchain();
            _blockManager = new BlockManager();
            _merkleTree = new MerkleTree();
            lastTransactionTime = DateTime.Now;
            _dbProcessor = new DbProcessor();
        }
        public async Task<Block> StartBuilding()
        {
            try
            {
                
                await _blockChain.LoadBlocksFromBd();
                
                if (await TryBuildBlock())
                {
                    if (_blockChain.GetLastIndexAsync() == null)
                    {
                        Logger.Log("Blockchain is empty. Starting to create the genesis block.", LogLevel.Information, Source.Blockchain);
                        Block genesisBlock = await CreateGenesisBlock();
                        Logger.Log($"Genesis block created: {genesisBlock.Id}", LogLevel.Information, Source.Blockchain);
                        return genesisBlock;
                    }
                    
                    Block block = await BuildBlockAsync();
                    Logger.Log($"Block created: {block.Id}",LogLevel.Information, Source.Blockchain);
                    
                    await _blockChain.AddBlockAsync(block);
                    Logger.Log($"Block #{block.Index} added to blockchain.", LogLevel.Information, Source.Blockchain);

                    BlockModel blockModel = block.ToBlockModel();
            
                    await _dbProcessor.ProcessService<bool>(new BlocksBdService(new AppDbContext()), CommandType.Add, new DbData(blockModel));
                    BlockChainCore blockChainCore = new BlockChainCore();
                    await blockChainCore.RemoveTransactionsFromBlockFromSqlLite(block);
                    return block;
                }
                else
                {
                    Logger.Log("Conditions for building a block not met.", LogLevel.Warning, Source.Blockchain);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during block building process: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

        public async Task<bool> TryBuildBlock()
        {
            try
            {
                await _transactionMemoryPool.FillFromSqlLite();
                int transactionCount = _transactionMemoryPool.GetTransactionCount();
                Logger.Log($"Count transactions: {transactionCount.ToString()} wait for {MaxTransactionLimit}", LogLevel.Information, Source.Blockchain);
                if (_transactionMemoryPool.GetTransactionCount() >= MaxTransactionLimit || (DateTime.UtcNow - lastTransactionTime).TotalSeconds > MaxWaitTimeInSeconds)
                {
                    lastTransactionTime = DateTime.UtcNow;
                    return true;
                }
                Logger.Log("Waiting for more transactions to build the block.", LogLevel.Warning, Source.Blockchain);
                
               
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in TryBuildBlock: {ex.Message}", LogLevel.Error, Source.Blockchain);
                return false;
            }
        }
        
        private async Task<Block> BuildBlockAsync()
        {
            try
            {
               // string previousBlockHash = await _blockChain.GetLastBlockHashAsync();
               
               
               
               
                Block lastBlock = await _blockChain.GetLastBlockAsync();
                Logger.Log($"Last block : {lastBlock.Id}", LogLevel.Error, Source.Blockchain);
                string previousBlockHash = lastBlock.Hash;
                
                
                
                if (previousBlockHash == null) Logger.Log("Blockchain is empty", LogLevel.Warning, Source.Blockchain);
                
               // int index = await _blockChain.GetLastIndexAsync() + 1;
               
               int index = lastBlock.Index + 1;
               int difficulty = GetCurrentDifficulty(index);                
               Block block = new Block(index, new List<Transaction>(), previousBlockHash, string.Empty, difficulty, string.Empty, string.Empty);
                
                
                while (block.Transactions.Count < MaxTransactionLimit && _transactionMemoryPool.GetTransactionCount() > 0)
                {
                    var tx = _transactionMemoryPool.GetHighestFeeTransaction();
                    tx.BlockId = block.Id;
                    block.Transactions.Add(tx);
                }
                TransactionManager _transactionManager = new TransactionManager();
                Transaction transactionReward = await _transactionManager.CreateAwardedTransaction(block);

                block.Transactions.Add(transactionReward);
                block.MerkleRoot = _merkleTree.CalculateMerkleRoot(block.Transactions);
                Logger.Log("Merkle root calculated.", LogLevel.Information, Source.Blockchain);

                block.Nonce = MineBlock(block);
                Logger.Log("Block mined.", LogLevel.Information, Source.Blockchain);
                
                block.Hash = block.GenerateHash();
                Logger.Log($"Block hash generated: {block.Hash}", LogLevel.Information, Source.Blockchain);
                
                block.Signature = SignBlock(block);
                Logger.Log("Block signature created.", LogLevel.Information, Source.Blockchain);
                
                block.Size = block.CalculateSize();
                Logger.Log($"Block size: {block.Size} bytes.", LogLevel.Information, Source.Blockchain);
                
                return block;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in BuildBlockAsync: {ex.Message}", LogLevel.Error, Source.Blockchain);
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
                Logger.Log($"Error in MineBlock: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

        private string GenerateNonce()
        {
            try
            {
                string nonce = Guid.NewGuid().ToString();
                Logger.Log($"Trying nonce: {nonce}", LogLevel.Information , Source.Blockchain);
                return nonce;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GenerateNonce: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }


        private string SignBlock(Block block)
        {
            try
            {
                Logger.Log("Signing block.", LogLevel.Information , Source.Blockchain);
                return _blockManager.SignBlock(block.Hash);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in SignBlock: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

        private async Task<Block> CreateGenesisBlock()
        {
            try
            {
                Logger.Log("Creating genesis block", LogLevel.Information, Source.Blockchain);

                
                int index = 0;
                string previousHash = "0";
                var data = "first transaction";
                
                MyPrivatePeerInfoModel myInfo = await _dbProcessor.ProcessService<MyPrivatePeerInfoModel>(new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));
                Transaction transaction = new Transaction("GenesisBlock", myInfo.AddressWallet, 1000000, 0, "signed", data, myInfo.PublicKey);
                List<Transaction> transactions = new List<Transaction>();
                transactions.Add(transaction);
                string merkleRoot = _merkleTree.CalculateMerkleRoot(transactions);
                int difficulty = 1;
                string nonce = GenerateNonce();

                string signature = "";
                
                Block genesisBlock = new Block(index, transactions, previousHash, merkleRoot, difficulty, nonce, signature);
                genesisBlock.Signature = SignBlock(genesisBlock);
                Logger.Log($"Genesis block created successfully with ID: {genesisBlock.Id}.", LogLevel.Information, Source.Blockchain);
                genesisBlock.Hash = "0";
                return genesisBlock;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while creating genesis block: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }
        public int GetCurrentDifficulty(int blockIndex)
        {
            return 2 + (blockIndex / 100);
        }
    }
}
