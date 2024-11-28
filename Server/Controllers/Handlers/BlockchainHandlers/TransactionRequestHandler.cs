// This class handles incoming HTTP requests related to blockchain transactions.
// It processes the request based on the HTTP method (GET, POST, UPDATE, DELETE),
// and interacts with the TransactionService to perform the appropriate action.
// If an unsupported method is received, it returns an error response indicating "Unknown Method."

using BlockchainLib; // Blockchain library that may handle blockchain-related operations
using PeerLib.Services; // Peer service library for handling peer-to-peer communication and transactions
using RRLib; // Library for handling response-related operations
using RRLib.Responses; // Responses for server communication

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
        public async Task<Response> HandleRequestAsync(Request request)
        {
            // Switch based on the HTTP method to call the appropriate service method
            switch (request.Method)
            {
                case "GET":
                    // Handle the GET request by retrieving the transactions
                    return await _transactionService.GetTransactions(request);

                case "POST":
                    // Handle the POST request by posting a new transaction
                    return await _transactionService.PostTransactions(request);

                case "UPDATE":
                    // Handle the UPDATE request by updating an existing transaction
                    return await _transactionService.UpdateTransactions(request);

                case "DELETE":
                    // Handle the DELETE request by deleting a specified transaction
                    return await _transactionService.DeleteTransactions(request);

                // If an unknown HTTP method is received, return an error response
                default:
                    // Default response for unsupported methods
                    var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                    return response;
            }
        }
    }
}
