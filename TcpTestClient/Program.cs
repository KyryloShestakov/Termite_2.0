using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class TcpClientExample
{
    private string _serverIp;
    private int _serverPort;

    public TcpClientExample(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
    }

    // Асинхронный метод для отправки запросов
    public async Task SendRequestsAsync()
    {
        try
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                // Подключаемся к серверу
                await tcpClient.ConnectAsync(_serverIp, _serverPort);
                Console.WriteLine("Connected to server...");

                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };

                // Отправляем три запроса подряд
                for (int i = 1; i <= 3; i++)
                {
                    string request = $"Request {i}";
                    Console.WriteLine($"Sending: {request}");
                    await writer.WriteLineAsync(request);  // Отправляем запрос

                    // Получаем ответ от сервера
                    string response = await reader.ReadLineAsync();
                    Console.WriteLine($"Received: {response}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // IP и порт сервера
        string serverIp = "127.0.0.1";
        int serverPort = 5000;

        // Создаем экземпляр клиента и отправляем запросы
        var client = new TcpClientExample(serverIp, serverPort);
        await client.SendRequestsAsync();
    }
}