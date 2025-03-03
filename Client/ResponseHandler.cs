using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Utilities;

namespace Client;

public class ResponseHandler
{ 
    public async Task ReceiveResponse(TcpClient tcpClient)
    {
        try
        {
            NetworkStream stream = tcpClient.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string response = await reader.ReadLineAsync();
            Response responseObj = JsonConvert.DeserializeObject<Response>(response);
            Logger.Log($"Received response: {responseObj.Message}", LogLevel.Information, Source.Client);
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