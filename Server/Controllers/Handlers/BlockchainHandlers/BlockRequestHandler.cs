using BlockchainLib;
using RRLib;
using RRLib.Requests.BlockchainRequests;
using RRLib.Responses;

namespace Server.Controllers.Handlers.BlockchainHandlers;

public class BlockRequestHandler : IRequestHandler
{
    private BlockService _blockService;
    
    public BlockRequestHandler()
    {
        _blockService = new BlockService();
    }

    public async Task<Response> HandleRequestAsync(Request request)
    {
        BlockRequest blockRequest = request as BlockRequest ?? throw new InvalidOperationException();
        switch (request.Method)
        {
            case "GET":
                return await _blockService.GetBlocks(blockRequest);
            case "GETBYID":
                return await _blockService.GetBlockById(blockRequest);
            case "POST":
                return await _blockService.PostBlocks(blockRequest);
            case "UPDATE":
                return await _blockService.UpdateBlocks(blockRequest);
            case "DELETE":
                return await _blockService.DeleteBlocks(blockRequest);
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}