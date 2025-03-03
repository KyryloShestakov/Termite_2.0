using BlockchainLib.Blocks;
using DataLib.DB.SqlLite.Interfaces;
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
    private IDbProcessor _dbProcessor;
    public BlockChainCore()
    {
        _blockchain = new Blockchain();
        _blockBuilder = new BlockBuilder();
        _dbProcessor = new DbProcessor();
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
            
            await _dbProcessor.ProcessService<bool>(new BlocksBdService(new AppDbContext()), CommandType.Add, new DbData(blockModel));

            await RemoveTransactionsFromBlockFromSqlLite(block);


        }
        catch (Exception ex)
        {
            Logger.Log($"Error occurred while starting blockchain: {ex.Message}", LogLevel.Error, Source.Blockchain);
        }
    }

    
    public async Task RemoveTransactionsFromBlockFromSqlLite(Block block)
    {
        try
        {
            foreach (var transaction in block.Transactions)
            {
                await _dbProcessor.ProcessService<bool>(new TransactionBdService(new AppDbContext()), CommandType.Delete, new DbData(null, transaction.Id));
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error while removing transactions from Redis: {ex.Message}", LogLevel.Error, Source.Blockchain);
        }
    }

    
}