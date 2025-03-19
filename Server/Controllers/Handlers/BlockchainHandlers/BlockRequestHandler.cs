using BlockchainLib;
using BlockchainLib.Blocks;
using RRLib;
using RRLib.Responses;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace Server.Controllers.Handlers.BlockchainHandlers;

public class BlockRequestHandler : IRequestHandler
{
    private BlockService _blockService;
    
    public BlockRequestHandler()
    {
        _blockService = new BlockService();
    }

    public async Task<Response> HandleRequestAsync(TerProtocol<object> request)
    {
        
        string json = request.Payload.Serialize();
        var obj = RequestSerializer.DeserializeData(request.Header.MessageType,json);
            
        BlockRequest bRequest = (BlockRequest)obj;
        
        switch (obj)
        {
            case BlockRequest blockRequest:
                TerProtocol<object> terProtocol = new TerProtocol<object>(request.Header, new TerPayload<object>(bRequest));
                switch (request.Header.MethodType)
                {
                    case MethodType.Post:
                        await _blockService.PostBlocks(terProtocol);
                        break;
                                    
                    default:
                        Logger.Log("Unknown command.", LogLevel.Warning, Source.Server);
                        break;
                }
                break;
                    
            default:
                Console.WriteLine("Unsupported request type.");
                break;
        }
        return new Response();
    }
}