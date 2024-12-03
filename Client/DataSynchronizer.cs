using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using StorageLib.DB.SqlLite.Services;
using NetworkLibrary.Networking.Client.Requests;

namespace Client;

public class DataSynchronizer
{
   private TcpClient _tcpClient;
   
   private RequestHandler _requestHandler;
   private ResponseHandler _responseHandler;
   
   public DataSynchronizer(TcpClient tcpClient)
   {
      _tcpClient = tcpClient;
      _requestHandler = new RequestHandler();
      _responseHandler = new ResponseHandler();

      StartAsync(tcpClient);
   }

   private async void StartAsync(TcpClient tcpClient)
   {
      Stopwatch timer = Stopwatch.StartNew();
      
      
   }

   
   private async Task ProcessingNodeInfo(TcpClient tcpClient)
   {
     // List<Request> requests = new List<Request>();
     // await _requestHandler.AddRequestsToList(requests);
     // await _requestHandler.SendRequestAsync(tcpClient);
     // await _responseHandler.ReceiveResponse(tcpClient);
     //  List<Response> responses = await _responseHandler.getResponses();
   }
   
   
}