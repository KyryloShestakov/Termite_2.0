using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TermiteUI.Pages.Blockchain;

public partial class BlockchainView : UserControl
{
    
    
    public BlockchainView()
    {
        InitializeComponent();
        DataContext = new BlockchainViewModel();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    
   
}