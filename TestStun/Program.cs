using System;
using System.Net;
using System.Net.Sockets;

class Program
{
    static void Main()
    {
        try
        {
            string stunServer = "stun.l.google.com";
            int stunPort = 19302;

            using (var client = new UdpClient())
            {
                client.Connect(stunServer, stunPort);

                byte[] stunRequest = CreateStunBindingRequest();
                client.Send(stunRequest, stunRequest.Length);
                Console.WriteLine("STUN-запрос отправлен.");

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, stunPort);
                byte[] response = client.Receive(ref remoteEP);

                if (response.Length > 0)
                {
                    Console.WriteLine("Ответ STUN-сервера:");
                    Console.WriteLine(BitConverter.ToString(response));

                    ParseStunResponse(response);
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

    private static byte[] CreateStunBindingRequest()
    {
        byte[] transactionId = new byte[12];
        new Random().NextBytes(transactionId);

        byte[] request = new byte[20];
        request[0] = 0x00; request[1] = 0x01;
        request[2] = 0x00; request[3] = 0x00;
        request[4] = 0x21; request[5] = 0x12;
        request[6] = 0xA4; request[7] = 0x42;

        Buffer.BlockCopy(transactionId, 0, request, 8, 12);
        return request;
    }

    private static void ParseStunResponse(byte[] response)
    {
        const int MAGIC_COOKIE = 0x2112A442;
        int index = 20; // Пропускаем заголовок

        while (index < response.Length)
        {
            int type = (response[index] << 8) | response[index + 1];
            int length = (response[index + 2] << 8) | response[index + 3];
            index += 4;

            if (type == 0x0020) // XOR-MAPPED-ADDRESS
            {
                int family = response[index + 1];
                int xorPort = ((response[index + 2] << 8) | response[index + 3]) ^ (MAGIC_COOKIE >> 16);
                int xorIP = BitConverter.ToInt32(response, index + 4) ^ MAGIC_COOKIE;

                IPAddress externalIP = new IPAddress(BitConverter.GetBytes(xorIP));
                Console.WriteLine($"Внешний IP: {externalIP}, Порт: {xorPort}");
                return;
            }

            index += length;
        }

        Console.WriteLine("Не удалось извлечь внешний IP.");
    }
}
