using RRLib;
using RRLib.Responses;

namespace Server.Controllers.Handlers;

/// <summary>
/// Interface defining a request handler responsible for processing specific types of requests.
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// Handles the provided request asynchronously and returns a response.
    /// </summary>
    /// <param name="request">The request object containing the necessary information for processing.</param>
    /// <returns>A <see cref="Response"/> object representing the result of the request handling.</returns>
    Task<Response> HandleRequestAsync(Request request);
}