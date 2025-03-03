using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlockchainLib.Addresses;

namespace TermiteUI;

public partial class WalletPage : UserControl, INotifyPropertyChanged
{
    public WalletPage()
    {
        InitializeComponent();
        _addressManager = new AddressManager();
        Balance = "Enter an address and click 'Get Balance'";
        DataContext = this;
        
    }
    
    private string _address;
    private string _balance;
    private readonly AddressManager _addressManager; // Интерфейс вашей библиотеки
    public string Address
    {
        get => _address;
        set
        {
            _address = value;
            OnPropertyChanged();
        }
    }

    public string Balance
    {
        get => _balance;
        set
        {
            _balance = value;
            OnPropertyChanged();
        }
    }

    public async Task GetBalanceAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                Balance = "Address cannot be empty.";
                return;
            }

            Balance = "Loading...";
            var balanceResult = await _addressManager.GetBalance(Address); // Асинхронный метод библиотеки
            Balance = $"Balance: {balanceResult} TER"; // Например: 1.2345 BTC
        }
        catch (Exception ex)
        {
            Balance = $"Error: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

