using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using BlockchainLib;
using StorageLib.DB.Redis;
using Utilities;

namespace TermiteUI;

public partial class MinePage : UserControl, INotifyPropertyChanged
{
    private int _unconfirmedTransactionsCount;
    private Timer _timer;

    public int UnconfirmedTransactionsCount
    {
        get => _unconfirmedTransactionsCount;
        set
        {
            if (_unconfirmedTransactionsCount != value)
            {
                _unconfirmedTransactionsCount = value;
                OnPropertyChanged();
            }
        }
    }

    public MinePage()
    {
        InitializeComponent();
        DataContext = this;
        GetCountUnconfirmedTransactions();
    }

    public async Task StartMineAsync()
    {
        BlockChainCore blockChain = new BlockChainCore();
        _timer = new Timer(TimerCallback, blockChain, 0, 60000);
    }

    private void StopMining()
    {
        _timer?.Dispose();
        Logger.Log("Mining stopped", LogLevel.Information, Source.Blockchain);
    }

    private async void GetCountUnconfirmedTransactions()
    {
        TransactionMemoryPool _transactionMemoryPool = new TransactionMemoryPool();
        await Task.Run(() => _transactionMemoryPool.FillFromSqlLite());
        int num = _transactionMemoryPool.GetTransactionCount();
        Logger.Log($"Number of unconfirmed transactions: {num}", LogLevel.Information, Source.Blockchain);
        UnconfirmedTransactionsCount = num;
    }

    private static void TimerCallback(object state)
    {
        try
        {
            BlockChainCore blockChain = (BlockChainCore)state;
            blockChain.StartBlockchain();
            Logger.Log("Blockchain processed", LogLevel.Information, Source.Blockchain);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error in blockchain processing: {ex.Message}", LogLevel.Error, Source.Blockchain);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
