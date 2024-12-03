using System.Net.Sockets;
using System.Text;
using Utilities;

namespace Client;

public class ResponseHandler
{
    private List<Response> _responsesList;
    public ResponseHandler()
    {
        _responsesList = new List<Response>();
    }

    public async Task ReceiveResponse(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] responseBuffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
        if (bytesRead > 0)
        {
            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            Logger.Log("Response received: " + response);
            await AddResponse(response);
        }
    }

    private async Task AddResponse(string message)
    {
        var response = new Response();
        response.Message = message;
        _responsesList.Add(response);
    }

    public async Task<List<Response>> getResponses()
    {
        //TODO после того как я забираю ответы от сервера мне надо подсчишать список
        return _responsesList;
    }
}