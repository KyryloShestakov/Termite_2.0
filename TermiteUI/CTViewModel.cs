using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace TermiteUI;

public class CTViewModel : UserControl
{
    public ObservableCollection<TransactionModel> Transactions { get; set; }

    private readonly BlocksBdService _blockService;

    public CTViewModel()
    {
        _blockService = new BlocksBdService(new AppDbContext());
        Transactions = new ObservableCollection<TransactionModel>();
        
        LoadTransactionsAsync();
        Logger.Log($"{Transactions.Count} Transactions", LogLevel.Information, Source.App);
        
    }

    private async Task LoadTransactionsAsync()
    {
        try
        {
            var blocks = await _blockService.GetAllBlocksAsync();

           
                foreach (var block in blocks)
                {
                    var transactions = GetTransactionsFromBlock(block);
                    foreach (var transaction in transactions)
                    {
                        Transactions.Add(transaction);
                        Logger.Log($"Transaction was added {transaction.Id}", LogLevel.Information, Source.App);
                    }
                }

                Logger.Log($"Total transactions loaded: {Transactions.Count}", LogLevel.Information, Source.App);
                
        }
        catch (Exception ex)
        {
            Logger.Log($"Error loading transactions: {ex.Message}", LogLevel.Error, Source.App);
        }
    }

    private List<TransactionModel> GetTransactionsFromBlock(BlockModel block)
    {
        var transactions = new List<TransactionModel>();

        if (!string.IsNullOrEmpty(block.Transactions))
        {
            try
            {
                transactions = JsonConvert.DeserializeObject<List<TransactionModel>>(block.Transactions);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deserializing transactions: {ex.Message}", LogLevel.Error, Source.App);
            }
        }

        return transactions;
    }


}