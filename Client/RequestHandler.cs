using System.Net.Sockets;
using System.Text;
using NetworkLibrary.Networking.Client.Requests;
using Utilities;

namespace Client;

public class RequestHandler
{
    public RequestHandler()
    {
    }

    public async Task SendRequestAsync(TcpClient client, string request)
    {
        
        if (!client.Connected)
        {
            Logger.Log("Client is not connected", LogLevel.Error, Source.Client);
            throw new InvalidOperationException("Client is not connected");
        }

        try
        {
            Logger.Log($"Sending request: {request}");

            NetworkStream stream = client.GetStream();

            if (!stream.CanWrite)
            {
                Logger.Log("Network stream is not writable", LogLevel.Error, Source.Client);
                throw new InvalidOperationException("Network stream is not writable");
            }

            byte[] dataToSend = Encoding.UTF8.GetBytes(request);

            if (client.Connected)
            {
                await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
                Logger.Log($"Message sent {dataToSend.Length}", LogLevel.Information, Source.Client);

            }
            else
            {
                Logger.Log("Connection is not active. Cannot send data.", LogLevel.Warning, Source.Client);
            }
        }
        catch (SocketException socketEx)
        {
            Logger.Log($"SocketException while sending message: {socketEx.Message}", LogLevel.Error, Source.Client);
            throw;
        }
        catch (IOException ioEx)
        {
            Logger.Log($"IOException while sending message: {ioEx.Message}", LogLevel.Error, Source.Client);
            throw;
        }
        catch (Exception ex)
        {
            Logger.Log($"Error sending message: {ex.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }


}