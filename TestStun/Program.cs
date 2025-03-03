using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        try
        {
            // Адрес и порт STUN-сервера
            string stunServer = "23.21.150.121"; // Замените на адрес нужного STUN-сервера
            int stunPort = 3478;                   // STUN обычно слушает 19302 (UDP и TCP)
            
            // Устанавливаем соединение
            using (var client = new TcpClient())
            {
                client.Connect(stunServer, stunPort); // Подключаемся по TCP
                var stream = client.GetStream();

                // Формируем STUN-запрос
                byte[] stunRequest = CreateStunBindingRequest();

                // Отправляем запрос
                stream.Write(stunRequest, 0, stunRequest.Length);
                Console.WriteLine("STUN-запрос отправлен.");

                // Получаем ответ
                byte[] response = new byte[1024];
                int bytesRead = stream.Read(response, 0, response.Length);

                if (bytesRead > 0)
                {
                    Console.WriteLine("Ответ STUN-сервера:");
                    Console.WriteLine(BitConverter.ToString(response, 0, bytesRead));
                }
                else
                {
                    Console.WriteLine("Нет ответа от STUN-сервера.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    // Метод для создания STUN Binding Request
    private static byte[] CreateStunBindingRequest()
    {
        // Тип сообщения: Binding Request (0x0001)
        // Magic Cookie: 0x2112A442
        // Transaction ID: 16 байт случайных данных
        byte[] transactionId = new byte[12];
        new Random().NextBytes(transactionId);

        // Формируем бинарный запрос
        byte[] request = new byte[20]; // Заголовок STUN — 20 байт
        request[0] = 0x00;  // Тип сообщения (старший байт)
        request[1] = 0x01;  // Тип сообщения (младший байт)
        request[2] = 0x00;  // Длина (старший байт)
        request[3] = 0x00;  // Длина (младший байт)
        request[4] = 0x21;  // Magic Cookie (часть 1)
        request[5] = 0x12;  // Magic Cookie (часть 2)
        request[6] = 0xA4;  // Magic Cookie (часть 3)
        request[7] = 0x42;  // Magic Cookie (часть 4)

        // Добавляем Transaction ID
        Buffer.BlockCopy(transactionId, 0, request, 8, 12);

        return request;
    }
}
