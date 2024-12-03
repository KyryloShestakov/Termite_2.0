using RRLib;
using RRLib.Responses;
using Server.Controllers.Handlers;
using Utilities;

namespace Server.Controllers
{
    /// <summary>
    /// This class is responsible for handling incoming requests. It processes requests by
    /// delegating them to appropriate request handlers based on the request type.
    /// </summary>
    public class Controller : IController
    {
        // Factory that provides the appropriate request handler based on request type
        private RequestHandlerFactory _requestHandlerFactory;

        // Constructor initializes the request handler factory
        public Controller()
        {
            _requestHandlerFactory = new RequestHandlerFactory();
        }

        /// <summary>
        /// Handles the incoming request asynchronously by finding the appropriate handler 
        /// and delegating the request to it. If no handler is found or an error occurs, 
        /// a response indicating failure is returned.
        /// </summary>
        /// <param name="request">The incoming request to be handled.</param>
        /// <returns>Returns a Response object, which indicates the result of processing the request.</returns>
        public async Task<Response> HandleRequestAsync(Request request)
        {
            try
            {
                // Get the handler for the specific request type
                var handler = _requestHandlerFactory.GetHandler(request.RequestType);
                
                // If a handler is found, handle the request and return the response
                if (handler != null)
                {
                    Response response = await handler.HandleRequestAsync(request);
                    return response;
                }
                else
                {
                    // If no handler is found, log the error and return a failure response
                    Logger.Log("Handler could not be found", LogLevel.Error, Source.Server);
                    return new ServerResponseService().GetResponse(false, "Handler not found");
                }
            }
            catch (Exception e)
            {
                // Log the exception and rethrow it
                Logger.Log($"{e}", LogLevel.Error, Source.Server);
                throw;
            }
        }
    }
}
