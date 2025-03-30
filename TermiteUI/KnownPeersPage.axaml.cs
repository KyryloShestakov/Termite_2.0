using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CoreLib;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using EnumsLib;
using ModelsLib;
using ModelsLib.NetworkModels;
using PeerLib.Services;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;

namespace TermiteUI;

public partial class KnownPeersPage : UserControl, INotifyPropertyChanged
{
    private ObservableCollection<PeerInfoModel> _peers;
    private readonly AppDbContext _dbContext;
    private readonly IDbProcessor _dbProcessor;

    public ObservableCollection<PeerInfoModel> Peers
    {
        get => _peers;
        set
        {
            _peers = value;
            OnPropertyChanged(nameof(Peers));
        }
    }

    public KnownPeersPage()
    {
        _dbProcessor = new DbProcessor();
        InitializeComponent();
        DataContext = this;
        Peers = new ObservableCollection<PeerInfoModel>();
        _dbContext = new AppDbContext();
        _ = LoadPeers();
    }

    private async Task LoadPeers()
    {
        try
        {
            List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new PeerInfoService(_dbContext), CommandType.GetAll);
            List<PeerInfoModel> peers = models.Cast<PeerInfoModel>().ToList();
            foreach (var peersInfo in peers)
            {
                Peers.Add(peersInfo);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading peers: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}