using System.Net.Sockets;
using System.Text;
using Utilities;

namespace Client;

public class ResponseHandler
{
    public ResponseHandler()
    {
    }

    public async Task ReceiveResponse(TcpClient tcpClient)
    {
        try
        {
            NetworkStream stream = tcpClient.GetStream();
            byte[] buffer = new byte[1024];
            StringBuilder responseMessage = new StringBuilder();
            int bytesRead;

            // Чтение из потока до получения ответа
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                responseMessage.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }

            Logger.Log($"Received response: {responseMessage.ToString()}", LogLevel.Information, Source.Client);
        }
        catch (OperationCanceledException)
        {
            Logger.Log("Operation was canceled while receiving the response.", LogLevel.Warning, Source.Client);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error while receiving response: {ex.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }


   
}