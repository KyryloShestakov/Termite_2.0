using System.Net.Sockets;
using Client.Synchronization;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Requests.NetRequests;
using Utilities;

namespace Client;

public class RequestExecutor
{
    private RequestHandler _requestHandler;
    private ResponseHandler _responseHandler;
    
    public RequestExecutor()
    {
        _requestHandler = new RequestHandler();
        _responseHandler = new ResponseHandler();
    }

    public async Task StartExecution(TcpClient tcpClient, RequestPool _requestPool)
    {
        try
        {
            Logger.Log("Starting request processing", LogLevel.Information, Source.Client);
            
            var requests = _requestPool.GetAllRequests();
        
            if (requests.Count == 0)
            {
                Logger.Log("No requests available", LogLevel.Warning, Source.Client);
                return;  
            }

            Logger.Log($"Request pool contains {requests.Count} requests.", LogLevel.Information, Source.Client);

            
                foreach (var request in requests)
                {
                    switch (request.RequestType)
                    {
                        case "PeerInfo":
                            PeerInfoRequest peerInfoRequest = request as PeerInfoRequest;
                            string peerInfoRequestJson = peerInfoRequest.Serialize();
                            await _requestHandler.SendRequestAsync(tcpClient, peerInfoRequestJson);
                            await _responseHandler.ReceiveResponse(tcpClient);
                            break;
                        case "KeyExchange":
                            KeyExchangeRequest keyExchangeRequest = request as KeyExchangeRequest;
                            string keyExchangeRequestJson = keyExchangeRequest.Serialize();
                            await _requestHandler.SendRequestAsync(tcpClient, keyExchangeRequestJson);
                            await _responseHandler.ReceiveResponse(tcpClient);
                            break;
                        case "KnownPeers":
                            KnownPeersRequest knownPeersRequest = request as KnownPeersRequest;
                            string knownPeersRequestJson = knownPeersRequest.Serialize();
                            await _requestHandler.SendRequestAsync(tcpClient, knownPeersRequestJson);
                            await _responseHandler.ReceiveResponse(tcpClient);
                            break;
                        case "Block":
                            BlockRequest blockRequest = request as BlockRequest;
                            string blockRequestJson = blockRequest.Serialize();
                            await _requestHandler.SendRequestAsync(tcpClient, blockRequestJson);
                            await _responseHandler.ReceiveResponse(tcpClient);
                            break;
                        case "Transaction":
                            Task.Delay(1000).Wait();
                            TransactionRequest transactionRequest = request as TransactionRequest;
                            string transactionRequestJson = transactionRequest.Serialize();
                            await _requestHandler.SendRequestAsync(tcpClient, transactionRequestJson);
                            await _responseHandler.ReceiveResponse(tcpClient);

                            break;
                    }
                }
            
        }
        catch (Exception e)
        {
            Logger.Log($"Error in request execution: {e.Message}", LogLevel.Error, Source.Client);
            throw;
        }
    }

}