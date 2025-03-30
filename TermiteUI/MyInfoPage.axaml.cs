using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CoreLib;
using ModelsLib.NetworkModels;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services;
using Utilities;
using System.ComponentModel;
using Avalonia.Markup.Xaml;
using DataLib.DB.SqlLite.Interfaces;
using DataLib.DB.SqlLite.Services.NetServices;
using ModelsLib;

namespace TermiteUI;

public partial class MyInfoPage : UserControl, INotifyPropertyChanged
{
    private readonly IDbProcessor _dbProcessor;
    // Подписка на события изменения свойств
    public event PropertyChangedEventHandler PropertyChanged;

    private MyPrivatePeerInfoModel _peerInfo;
    public MyPrivatePeerInfoModel PeerInfo 
    { 
        get => _peerInfo; 
        set 
        {
            if (_peerInfo != value)
            {
                _peerInfo = value;
                OnPropertyChanged(nameof(PeerInfo));
            }
        }
    }

    public MyInfoPage()
    {
        InitializeComponent();
        _dbProcessor = new DbProcessor();
        DataContext = this;
        _peerInfo = new MyPrivatePeerInfoModel();
        // Получаем данные после инициализации
        _ = GetMyPrivatePeerInfoAsync();
    }

    // Асинхронный метод для создания Peer
    public async Task CreatePeerAsync()
    {
        try
        {
            var peer = new Peer();
            await peer.CreatePeer(); // Сделать метод асинхронным, если внутри есть длительные операции
        }
        catch (Exception ex)
        {
            // Обработка ошибок, например, вывод в консоль
            Logger.Log($"Error when creating Peer: {ex.Message}", LogLevel.Error, Source.App);
        }
    }

    // Асинхронный метод для получения информации о Peer
    private async Task GetMyPrivatePeerInfoAsync()
    {
        try
        {
            IModel model = await _dbProcessor.ProcessService<IModel>(new MyPrivatePeerInfoService(new AppDbContext()), CommandType.Get, new DbData(null, "default"));
            MyPrivatePeerInfoModel peerInfo = model as MyPrivatePeerInfoModel ?? throw new InvalidOperationException();

            if (peerInfo != null)
            {
                PeerInfo = peerInfo;
            }
            else
            {
                Console.WriteLine("Ошибка: не удалось преобразовать объект в MyPrivatePeerInfoModel.");
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок
            Console.WriteLine($"Ошибка при получении информации о Peer: {ex.Message}");
        }
    }

    // Метод для уведомления UI о изменении свойства
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}
