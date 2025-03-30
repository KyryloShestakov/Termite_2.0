// This class handles incoming HTTP requests related to blockchain transactions.
// It processes the request based on the HTTP method (GET, POST, UPDATE, DELETE),
// and interacts with the TransactionService to perform the appropriate action.
// If an unsupported method is received, it returns an error response indicating "Unknown Method."

using BlockchainLib;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json; // Blockchain library that may handle blockchain-related operations
using PeerLib.Services; // Peer service library for handling peer-to-peer communication and transactions
using RRLib; // Library for handling response-related operations
using RRLib.Responses;
using Ter_Protocol_Lib.Requests;
using Utilities; // Responses for server communication

namespace Server.Controllers.Handlers.BlockchainHandlers
{
    // Handler for processing transaction-related HTTP requests
    public class TransactionRequestHandler : IRequestHandler
    {
        // Private instance of TransactionService used to handle various transaction operations
        private TransactionService _transactionService { get; }

        // Constructor initializes the TransactionService instance
        public TransactionRequestHandler()
        {
            _transactionService = new TransactionService();
        }

        // Asynchronous method that processes the incoming request based on the HTTP method
        public async Task<Response> HandleRequestAsync(TerProtocol<object> request)
        {
            string json = request.Payload.Serialize();
            Logger.Log(json, LogLevel.Warning, Source.Server);
            var obj = RequestSerializer.DeserializeData(request.Header.MessageType,json);
            
            TransactionRequest txRequest = (TransactionRequest)obj;
            if (txRequest != null)
            {
                TerProtocol<object> terTxProtocol = new TerProtocol<object>(request.Header, new TerPayload<object>(txRequest));
            
                switch (request.Header.MethodType)
                {
                    case MethodType.Post:
                        Response txPostResponse = await _transactionService.PostTransactions(terTxProtocol);
                        return txPostResponse;
                    case MethodType.Get:
                        Response txGetResponse = await _transactionService.GetTransaction(terTxProtocol);
                        return txGetResponse;
                                    
                    default:
                        Logger.Log("Unknown command.", LogLevel.Warning, Source.Server);
                        break;
                    
                }
            }

            return new ServerResponseService().GetResponse(true, "Request processed successfully.");
        }
    }
}
