using BlockchainLib.Blocks;
using ModelsLib.BlockchainLib;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace BlockchainLib;

public class BlockChainCore
{
    private Blockchain _blockchain;
    private BlockBuilder _blockBuilder;
    private BlocksBdService _blocksBdService;
    
    public BlockChainCore()
    {
        _blockchain = new Blockchain();
        _blockBuilder = new BlockBuilder();
    }

    public async void StartBlockchain()
    {
        try
        {
            Logger.Log("Starting blockchain...");

            // Запускаем сбор блока
            Block block = await _blockBuilder.StartBuilding();
            if (block == null)
            {
                Logger.Log("Block creation failed. Aborting blockchain start.");
                return;
            }

            Logger.Log($"Block #{block.Index} successfully created. Adding block to blockchain...");

            // Добавляем блок в блокчейн
            await _blockchain.AddBlockAsync(block);

            Logger.Log($"Block #{block.Index} added to blockchain.");

            // Преобразуем блок в модель
            BlockModel blockModel = block.ToBlockModel();

            // Добавляем блок в базу данных
            await _blocksBdService.AddBlockAsync(blockModel);

            Logger.Log($"Block #{block.Index} added to database.");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error occurred while starting blockchain: {ex.Message}");
        }
    }

    
}