
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;


namespace TermiteUI;

public partial class BlockchainPage : UserControl
{
    
    
    public BlockchainPage()
    {
        InitializeComponent();
        DataContext = new BlockchainViewModel();
       
    }
    
   
}