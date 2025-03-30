using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TermiteUI.Pages.Blockchain
{
    public partial class TransactionView : UserControl
    {


        public TransactionView()
        {
            InitializeComponent();
            DataContext = new TransactionModel();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
    
    
