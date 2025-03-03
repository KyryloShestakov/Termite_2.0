using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace TermiteUI;

public class BlockchainViewModel
{
    public ObservableCollection<BlockModel> Blocks { get; set; }
    private readonly IDbProcessor _dbProcessor;
    private readonly AppDbContext _appDbContext;

    public BlockchainViewModel()
    {
        _dbProcessor = new DbProcessor();
        Blocks = new ObservableCollection<BlockModel>();
        Logger.Log($"{Blocks.Count} blocks", LogLevel.Information, Source.App);
        LoadBlocks();
        _appDbContext = new AppDbContext();
        

    }
    
    private async Task LoadBlocks()
    {
        try
        {
            if (_dbProcessor == null)
            {
                Logger.Log("Error: _dbProcessor is null", LogLevel.Error, Source.App);
                return;
            }

            var dbContext = new AppDbContext();
            if (dbContext == null)
            {
                Logger.Log("Error: AppDbContext is null", LogLevel.Error, Source.App);
                return;
            }

            var service = new BlocksBdService(dbContext);
            if (service == null)
            {
                Logger.Log("Error: BlocksBdService is null", LogLevel.Error, Source.App);
                return;
            }

            List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
            List<BlockModel> blocks = models.Cast<BlockModel>().ToList();

            
            if (blocks == null)
            {
                Logger.Log("Warning: No blocks found", LogLevel.Warning, Source.App);
                return;
            }

            if (Blocks == null)
            {
                Logger.Log("Error: Blocks list is null", LogLevel.Error, Source.App);
                return;
            }

            foreach (var block in blocks)
            {
                Blocks.Add(block);
            }

            Logger.Log($"Total blocks loaded: {Blocks.Count}", LogLevel.Information, Source.App);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error loading blocks: {ex.Message}", LogLevel.Error, Source.App);
        }
    }

}