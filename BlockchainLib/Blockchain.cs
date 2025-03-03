using System.Collections.Immutable;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace BlockchainLib
{
    public class Blockchain
    {
        public ImmutableList<Block> Chain { get; private set; }

        public Blockchain()
        {
            Chain = ImmutableList<Block>.Empty;
        }

        public async Task AddBlockAsync(Block block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            Chain = Chain.Add(block);

            await Task.CompletedTask;
        }

        public async Task<Block> GetLastBlockAsync()
        {
            try
            {
                if (Chain == null || Chain.Count == 0)
                {
                    Logger.Log("Blockchain chain is empty or null.", LogLevel.Warning, Source.Server);
                    return await Task.FromResult<Block>(null);
                }

                Logger.Log("Successfully retrieved the last block.", LogLevel.Information, Source.Server);
                return await Task.FromResult(Chain[^1]);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error retrieving the last block: {ex.Message}", LogLevel.Error, Source.Server);
                throw;
            }
        }

        public async Task<string?> GetLastBlockHashAsync()
        {
            if (Chain == null || Chain.Count == 0)
            {
                Logger.Log("Blockchain chain is empty, returning null hash.", LogLevel.Warning, Source.Blockchain);
                return await Task.FromResult<string?>(null);
            }

            string hash = Chain[^1].Hash;
            Logger.Log($"Successfully retrieved the hash: {hash}", LogLevel.Information, Source.Blockchain);
            return hash;
        }


        public Task<int> GetLastIndexAsync()
        {
            return Task.FromResult(Chain.Count > 0 ? Chain[^1].Index : 0);
        }


        public async Task LoadBlocksFromBd()
        {
            IDbProcessor _dbProcessor = new DbProcessor();

            try
            {
                List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);

                List<BlockModel> blocks = models.Cast<BlockModel>().OrderBy(b => b.Index).ToList();

                foreach (var blockModel in blocks)
                {
                    Logger.Log($"Block #{blockModel.Index} was adeded", LogLevel.Warning, Source.Blockchain);
                    Block block = Block.FromBlockModel(blockModel); 
                    await this.AddBlockAsync(block);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error when loading blocks from the database: {ex.Message}", LogLevel.Error, Source.Blockchain);
            }
        }

    }
}