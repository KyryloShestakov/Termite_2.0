using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using API;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlockchainLib;
using BlockchainLib.Addresses;
using BlockchainLib.Blocks;
using Client;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib;
using ModelsLib.BlockchainLib;
using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace TermiteUI_2
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int _countOfBlocks;
        private string _idOfLastBlock;
        private string _balance;
        private TcpServer _tcpServer;
        private ClientTcp _clientTcp;
        private Timer _miningTimer;
        private ApiHost _apiServer;

        public string Balance
        {
            get => _balance;
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged(nameof(Balance));
                }
            }
        }

        public int CountOfBlocks
        {
            get => _countOfBlocks;
            set
            {
                if (_countOfBlocks != value)
                {
                    _countOfBlocks = value;
                    OnPropertyChanged(nameof(CountOfBlocks));
                }
            }
        }

        public string IdOfLastBlock
        {
            get => _idOfLastBlock;
            set
            {
                if (_idOfLastBlock != value)
                {
                    _idOfLastBlock = value;
                    OnPropertyChanged(nameof(IdOfLastBlock));
                }
            }
        }

        public RelayCommand StartNodeCommand { get; set; }

        public MainWindow()
        {
            // Команда инициализируется до установки DataContext
            StartNodeCommand = new RelayCommand(StartNode);

            InitializeComponent();
            DataContext = this;

            GetCountOfBlocks();
            GetBalance();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void GetCountOfBlocks()
        {
            IDbProcessor dbProcessor = new DbProcessor();
            List<IModel> models =
                await dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()),
                    CommandType.GetAll);
            List<BlockModel> blocks = models.Cast<BlockModel>().ToList();

            CountOfBlocks = models.Count;
            IdOfLastBlock = blocks.LastOrDefault()?.Id;
        }

        private async void GetBalance()
        {
            IDbProcessor dbProcessor = new DbProcessor();
            MyPrivatePeerInfoModel myInfo =
                await dbProcessor.ProcessService<MyPrivatePeerInfoModel>(
                    new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));
            AddressManager addressManager = new AddressManager();

            decimal balance = await addressManager.GetBalance(myInfo.AddressWallet);
            Balance = balance.ToString("F2");
        }

        public void StartNode()
        {
            // При необходимости включи сервер и клиент
            // StartServer();
            // StartClient();
            StartApi();
            StartMining();
            Logger.Log("Node started.", LogLevel.Information, Source.App);
        }

        private void StartServer()
        {
            _tcpServer = new TcpServer();
            _tcpServer.StartAsync();
            Logger.Log("Server started.", LogLevel.Information, Source.App);
        }

        private void StartClient()
        {
            _clientTcp = new ClientTcp();
            _clientTcp.RunAsync();
            Logger.Log("Client started.", LogLevel.Information, Source.App);
        }

        private void StartMining()
        {
            BlockChainCore blockChain = new BlockChainCore();
            _miningTimer = new Timer(TimerCallback, blockChain, 0, 10000);
            Logger.Log("Mining started.", LogLevel.Information, Source.App);
        }

        private static void TimerCallback(object state)
        {
            try
            {
                BlockBuilder blockBuilder = new BlockBuilder();
                blockBuilder.StartBuilding();
                Logger.Log($"Blockchain processed", LogLevel.Information, Source.Blockchain);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in blockchain processing: {ex.Message}", LogLevel.Error, Source.Blockchain);
            }
        }

        private void StartApi()
        {
            if (_apiServer == null)
            {
                _apiServer = new ApiHost();
                _apiServer.Start(); // метод запуска, который блокирует или асинхронный
                Logger.Log("API started.", LogLevel.Information, Source.App);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true);
        }

        public bool CanExecute(object parameter) => _canExecute();

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
