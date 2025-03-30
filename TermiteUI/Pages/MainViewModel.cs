using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TermiteUI.Pages.Blockchain;

namespace TermiteUI.Pages;


    public partial class MainViewModel : ObservableObject
    {
        // Свойство для текущего содержимого (страницы)
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Команды для переключения на страницы
        public IRelayCommand NavigateToPage1 { get; }
        public IRelayCommand NavigateToPage2 { get; }
        public IRelayCommand NavigateToPage3 { get; }
        public IRelayCommand NavigateToPage4 { get; }
        public IRelayCommand NavigateToPage5 { get; }
        
        public IRelayCommand NavigateToPage6 { get; }
        public IRelayCommand NavigateToPage7 { get; }

        public MainViewModel()
        {
            CurrentView = new NodeControlView();

            NavigateToPage1 = new RelayCommand(() => CurrentView = new NodeControlView());
            NavigateToPage2 = new RelayCommand(() => CurrentView = new MinerView());
            NavigateToPage3 = new RelayCommand(() => CurrentView = new TransactionView());
            NavigateToPage4 = new RelayCommand(() => CurrentView = new BlockchainView());
            NavigateToPage5 = new RelayCommand(() => CurrentView = new WalletView());
            NavigateToPage6 = new RelayCommand(() => CurrentView = new MyInfoPage());
            NavigateToPage7= new RelayCommand(() => CurrentView = new KnownPeersPage());
        }
    }
