using RRLib;
using Server.Controllers.Handlers.BlockchainHandlers;
using Server.Controllers.Handlers.NetHandlers;
using Ter_Protocol_Lib.Requests;

namespace Server.Controllers.Handlers;

/// <summary>
/// A factory class responsible for managing and providing the correct IRequestHandler based on the request type.
/// </summary>
public class RequestHandlerFactory
{
    // A dictionary that holds the mapping of request types to their corresponding handlers
    private readonly IDictionary<TerMessageType, IRequestHandler> _handlers;

    /// <summary>
    /// Constructor initializing the RequestHandlerFactory with predefined handlers for different request types.
    /// </summary>
    public RequestHandlerFactory()
    {
        _handlers = new Dictionary<TerMessageType, IRequestHandler>
        {
            // Mapping request types to their corresponding handler instances
            { TerMessageType.KeyExchange, new AuthRequestHandler() },
            { TerMessageType.Transaction, new TransactionRequestHandler() },
            { TerMessageType.Block, new BlockRequestHandler() },
            { TerMessageType.PeerInfo, new PeerInfoHandler() }
        };
    }

    /// <summary>
    /// Retrieves the appropriate IRequestHandler based on the request type.
    /// </summary>
    /// <param name="requestType">The type of the request (e.g., "KeyExchange", "KnownPeers", etc.).</param>
    /// <returns>An IRequestHandler instance corresponding to the given request type, or null if the handler is not found.</returns>
    public IRequestHandler GetHandler(TerMessageType requestType)
    {
        return _handlers.TryGetValue(requestType, out var handler) ? handler : throw new KeyNotFoundException($"Handler for {requestType} not found");
    }

}