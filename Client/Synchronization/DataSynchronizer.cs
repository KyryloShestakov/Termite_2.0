using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Sockets;
using Client.Synchronization;
using StorageLib.DB.SqlLite.Services;
using Newtonsoft.Json;
using RRLib;
using Utilities;

namespace Client;

public class DataSynchronizer
{
   private TcpClient _tcpClient;
   
   private RequestHandler _requestHandler;
   private ResponseHandler _responseHandler;
   private RequestPool _requestPool;
   private RequestFactory _requestFactory;
   private RequestExecutor _requestExecutor;
   
   public DataSynchronizer()
   {
      _requestHandler = new RequestHandler();
      _responseHandler = new ResponseHandler();
      _requestPool = new RequestPool();
      _requestFactory = new RequestFactory();
      _requestExecutor = new RequestExecutor();
   }

   public async Task StartSynchronization(TcpClient tcpClient)
   {
       try
       {
           Logger.Log("Starting synchronization...", LogLevel.Information, Source.Client);

           await _requestFactory.CreateKeyExchageRequest(_requestPool);
           await _requestFactory.CreatePeerInfoRequest(_requestPool);
           //await _requestFactory.CreateTransactionRequest(_requestPool);
           //Блоки
           //Известные узлы
           
           await _requestExecutor.StartExecution(tcpClient, _requestPool);
           
           Logger.Log("Request execution finished.", LogLevel.Information, Source.Client);
       }
       catch (Exception e)
       {
           Logger.Log($"Error during synchronization: {e}", LogLevel.Error, Source.Client);
           throw;
       }
   }

   


  
   
   
}