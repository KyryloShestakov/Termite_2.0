using RRLib;
using RRLib.Responses;

namespace Server.Controllers
{
    /// <summary>
    /// Interface that defines the contract for handling requests in the server.
    /// Classes implementing this interface should provide logic for processing requests.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Handles the incoming request asynchronously and returns a response.
        /// </summary>
        /// <param name="request">The request object containing details of the request.</param>
        /// <returns>A task representing the asynchronous operation, with a Response as the result.</returns>
        Task<Response> HandleRequestAsync(Request request);
    }
}