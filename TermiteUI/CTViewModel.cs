using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace TermiteUI;

public class CTViewModel : UserControl
{
    public ObservableCollection<TransactionModel> Transactions { get; set; }
    private readonly IDbProcessor _dbProcessor;
    private readonly AppDbContext _appDbContext;
    public CTViewModel()
    {
        Transactions = new ObservableCollection<TransactionModel>();
        _appDbContext = new AppDbContext();
        _dbProcessor = new DbProcessor();
        LoadTransactionsAsync();
        Logger.Log($"{Transactions.Count} Transactions", LogLevel.Information, Source.App);
        
    }

    private async Task LoadTransactionsAsync()
    {
        try
        {
            List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
            List<BlockModel> blocks = models.Cast<BlockModel>().ToList();
           
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