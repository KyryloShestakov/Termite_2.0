using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using ModelsLib.BlockchainLib;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace TermiteUI;

public class BlockchainViewModel
{
    public ObservableCollection<BlockModel> Blocks { get; set; }
    private readonly BlocksBdService _blockService;

    public BlockchainViewModel()
    {
        _blockService = new BlocksBdService(new AppDbContext());
        Blocks = new ObservableCollection<BlockModel>();
        Logger.Log($"{Blocks.Count} blocks", LogLevel.Information, Source.App);
        LoadBlocks();

    }
    
    private async Task LoadBlocks()
    {
        try
        {
            var blocks = await _blockService.GetAllBlocksAsync();

           
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