using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using Ter_Protocol_Lib.Requests;
using Utilities;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Client;

public class ResponseHandler
{
    public ResponseHandler()
    {
    }

    public async Task<Response> ReceiveResponse(TcpClient tcpClient)
    {
        try
        {
            if (tcpClient == null)
            {
                Logger.Log("TcpClient is null", LogLevel.Error, Source.Client);
                throw new ArgumentNullException("tcpClient is null");
            }

            Logger.Log("TcpClient is valid, attempting to get network stream...", LogLevel.Information, Source.Client);
            NetworkStream stream = tcpClient.GetStream();
        
            if (stream == null)
            {
                Logger.Log("Failed to get network stream from TcpClient", LogLevel.Error, Source.Client);
                throw new InvalidOperationException("Failed to get network stream");
            }

            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            Logger.Log("Attempting to read line from stream...", LogLevel.Information, Source.Client);
        
            string response = await reader.ReadLineAsync();
        
            if (string.IsNullOrEmpty(response))
            {
                Logger.Log("Received empty or null response", LogLevel.Warning, Source.Client);
                return null;
            }

            Logger.Log($"Raw response: {response}", LogLevel.Information, Source.Client);
            Response responseObj = JsonConvert.DeserializeObject<Response>(response);
        
            if (responseObj == null)
            {
                Logger.Log("Failed to deserialize response", LogLevel.Error, Source.Client);
                return null;
            }

            Logger.Log($"Received response: {responseObj.Message}, {responseObj.Data}", LogLevel.Information, Source.Client);
            return responseObj;
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

        return null;
    }

}