using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TermiteUI.Pages;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        
       Button? button = this.FindControl<Button>("Button");
       
       
    }
    
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}