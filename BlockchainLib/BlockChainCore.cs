using BlockchainLib.Blocks;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using StorageLib.DB.Redis;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace BlockchainLib;

public class BlockChainCore
{
    private Blockchain _blockchain;
    private BlockBuilder _blockBuilder;
    private BlocksBdService _blocksBdService;
    private RedisService _redisService;
    
    public BlockChainCore()
    {
        _blockchain = new Blockchain();
        _blockBuilder = new BlockBuilder();
        _blocksBdService = new BlocksBdService(new AppDbContext());
        _redisService = new RedisService();
    }

    public async void StartBlockchain()
    {
        try
        {
            Logger.Log("Starting blockchain...", LogLevel.Information, Source.Blockchain );

            
            Block block = await _blockBuilder.StartBuilding();
            if (block == null)
            {
                Logger.Log("Block creation failed. Aborting blockchain start.", LogLevel.Error, Source.Blockchain);
                return;
            }

            Logger.Log($"Block #{block.Index} successfully created. Adding block to blockchain...", LogLevel.Information, Source.Blockchain);
            
            await _blockchain.AddBlockAsync(block);
            Logger.Log($"Block #{block.Index} added to blockchain.", LogLevel.Information, Source.Blockchain);

            BlockModel blockModel = block.ToBlockModel();
            
            await _blocksBdService.AddBlockAsync(blockModel);

            await RemoveTransactionsFromBlockFromRedis(block);


        }
        catch (Exception ex)
        {
            Logger.Log($"Error occurred while starting blockchain: {ex.Message}");
        }
    }

    
    public async Task RemoveTransactionsFromBlockFromRedis(Block block)
    {
        try
        {
            List<TransactionModel> transactionModels = new List<TransactionModel>();
        
            foreach (var transaction in block.Transactions)
            {
                TransactionModel transactionModel = new TransactionModel
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Sender = transaction.Sender,
                    Receiver = transaction.Receiver,
                    Timestamp = transaction.Timestamp
                };

                transactionModels.Add(transactionModel);
            }

            await _redisService.RemoveTransactionsFromRedisAsync(transactionModels);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error while removing transactions from Redis: {ex.Message}", LogLevel.Error, Source.Blockchain);
        }
    }

    
}