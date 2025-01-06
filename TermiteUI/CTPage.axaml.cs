using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace TermiteUI
{
    public partial class CTPage : UserControl
    {


        public CTPage()
        {
            InitializeComponent();
            DataContext = new CTViewModel();

        }
    }
}
    
    
