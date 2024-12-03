using System.Net.Sockets;
using System.Text;
using NetworkLibrary.Networking.Client.Requests;
using Utilities;

namespace Client;

public class RequestHandler
{
    private List<Request> _requestsList;
    public RequestHandler()
    {
        _requestsList = new List<Request>();
    }

    public async Task SendRequestAsync(TcpClient client)
    {
        foreach (Request request in _requestsList)
        {
            byte[] dataToSend = Encoding.UTF8.GetBytes(request.Message);
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(dataToSend, 0, dataToSend.Length);
            Logger.Log("Message sent: " + request);
        }
    }

    public async Task AddRequestsToList(List<Request> requests)
    {
        foreach (var request in requests)
        {
            _requestsList.Add(request);
        }
    }
}