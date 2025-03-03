using Avalonia.Controls;
using Client;
using Server;
using Utilities;
using ReactiveUI;
using System.Reactive;
using CoreLib;

namespace TermiteUI
{
    public partial class Page1 : UserControl
    {
        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
        public ReactiveCommand<Unit, Unit> StartClientCommand { get; }
        public ReactiveCommand<Unit, Unit> StopServerCommand { get; }
        public ReactiveCommand<Unit, Unit> StopClientCommand { get; }

        public Page1()
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
    }
}