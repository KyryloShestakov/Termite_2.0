using Avalonia.Controls;

namespace TermiteUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();  // Устанавливаем ViewModel

    }
    
}