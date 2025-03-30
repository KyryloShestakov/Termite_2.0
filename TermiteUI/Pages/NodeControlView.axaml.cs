using System.Reactive;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Client;
using ReactiveUI;
using Utilities;

namespace TermiteUI.Pages
{
    public partial class NodeControlView : UserControl
    {
        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
        public ReactiveCommand<Unit, Unit> StartClientCommand { get; }
        public ReactiveCommand<Unit, Unit> StopServerCommand { get; }
        public ReactiveCommand<Unit, Unit> StopClientCommand { get; }

        public NodeControlView()
        {
            InitializeComponent();
            DataContext = this;

            // Инициализация команд
            StartServerCommand = ReactiveCommand.Create(StartServer);
            StartClientCommand = ReactiveCommand.Create(StartClient);
            StopServerCommand = ReactiveCommand.Create(StopServer);
            StopClientCommand = ReactiveCommand.Create(StopClient);
        }

        public void StartServer()
        {
            TcpServer server = new TcpServer();
            server.StartAsync();
            Logger.Log("Server started.", LogLevel.Information, Source.App);
        }

        public void StartClient()
        {
            ClientTcp client = new ClientTcp();
            client.RunAsync();
            Logger.Log("Client started.", LogLevel.Information, Source.App);
        }

        public void StopServer()
        {
            TcpServer server = new TcpServer();
            server.Stop();
            Logger.Log("Server stopped.", LogLevel.Information, Source.App);
        }

        public void StopClient()
        {
            ClientTcp client = new ClientTcp();
            client.StopAsync();
            Logger.Log("Client stopped.", LogLevel.Information, Source.App);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}