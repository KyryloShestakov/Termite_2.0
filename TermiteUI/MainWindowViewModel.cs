using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;

namespace TermiteUI;


    public partial class MainWindowViewModel : ObservableObject
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

        public MainWindowViewModel()
        {
            CurrentView = new Page1();

            NavigateToPage1 = new RelayCommand(() => CurrentView = new Page1());
            NavigateToPage2 = new RelayCommand(() => CurrentView = new MinePage());
            NavigateToPage3 = new RelayCommand(() => CurrentView = new CTPage());
            NavigateToPage4 = new RelayCommand(() => CurrentView = new BlockchainPage());
        }
    }
