using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text.Json;
using RRLib;
using Server.Controllers;
using Server.Requests;
using Utilities;

namespace Server.Executors;

/// <summary>
/// The `RequestExecutor` class is responsible for handling and delegating incoming TCP requests 
/// based on their request group. It processes network (`Net`) and blockchain (`Blockchain`) 
/// related requests by utilizing respective executors (`NetExecutor`, `BlockchainExecutor`).
/// 
/// Features:
/// - Initializes and manages the execution flow for specific request groups.
/// - Handles exceptions for deserialization, network errors, and unknown request types.
/// - Logs detailed information and errors for debugging purposes.
/// 
/// Components:
/// - `NetExecutor`: Handles network-related requests.
/// - `BlockchainExecutor`: Handles blockchain-related requests.
/// 
/// Example:
/// Upon receiving a request, the class determines its group and delegates it to the appropriate 
/// executor, ensuring structured and modular handling of operations.
/// </summary>
public class RequestExecutor
{
    private TcpClient _tcpClient;
    private TcpHandler _tcpHandler;
    private TcpRequest _tcpRequest;
    
    private NetExecutor _netExecutor;
    private BlockchainExecutor _blockchainExecutor;
    
    public RequestExecutor(TcpClient tcpClient, TcpHandler tcpHandler, TcpRequest tcpRequest)
    {
        _tcpClient = tcpClient;
        _tcpHandler = tcpHandler;
        _tcpRequest = tcpRequest;

        _ = Execute(_tcpClient, _tcpHandler, _tcpRequest);
    }

    private async Task Execute(TcpClient tcpClient, TcpHandler tcpHandler, TcpRequest tcpRequest)
    {
         Logger.Log("Request executor started.", LogLevel.Information, Source.Server);

         try
         {
             Request request = Request.Deserialize(tcpRequest.Message) ?? throw new InvalidOperationException();
             Logger.Log($"Request group: {request.RequestGroup}", LogLevel.Information, Source.Server);

             switch (request.RequestGroup)
             {
                 case "Net":
                     try
                     {
                         Logger.Log("Initializing NetExecutor...", LogLevel.Information, Source.Server);
                         _netExecutor = new NetExecutor(tcpClient, tcpHandler, tcpRequest);
                         
                         
                     }
                     catch (Exception netEx)
                     {
                         Logger.Log($"Error in NetExecutor: {netEx.Message}", LogLevel.Error, Source.Server);
                         await tcpHandler.Write("Error processing network request.");
                     }

                     break;

                 case "Blockchain":
                     try
                     {
                         Logger.Log("Initializing BlockchainExecutor...", LogLevel.Information, Source.Server);
                         _blockchainExecutor = new BlockchainExecutor();
                     }
                     catch (Exception blockchainEx)
                     {
                         Logger.Log($"Error in BlockchainExecutor: {blockchainEx.Message}", LogLevel.Error,
                             Source.Server);
                         await tcpHandler.Write("Error processing blockchain request.");
                     }

                     break;

                 default:
                     Logger.Log($"Unknown request group: {request.RequestGroup}", LogLevel.Warning, Source.Server);
                     await tcpHandler.Write($"Unsupported request group: {request.RequestGroup}");
                     break;
             }
         }
         catch (JsonException jsonEx)
         {
             string error = $"JSON deserialization error: {jsonEx.Message}";
             Logger.Log(error, LogLevel.Error, Source.Server);
             await tcpHandler.Write("Invalid JSON format");
         }
         catch (Exception ex)
         {
             string error = $"Unexpected error: {ex.Message}";
             Logger.Log(error, LogLevel.Error, Source.Server);
             await tcpHandler.Write("Internal Server Error");
         }
         finally
         {
           //  tcpClient.Close();
          //   Logger.Log("Connection closed.", LogLevel.Information, Source.Server);
         }
    }

    
    
    
    
    

}