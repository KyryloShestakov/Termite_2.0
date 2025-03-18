using BlockchainLib;
using BlockchainLib.Blocks;
using RRLib;
using RRLib.Responses;
using Ter_Protocol_Lib;

namespace Server.Controllers.Handlers.BlockchainHandlers;

public class BlockRequestHandler : IRequestHandler
{
    private BlockService _blockService;
    
    public BlockRequestHandler()
    {
        _blockService = new BlockService();
    }

    public async Task<Response> HandleRequestAsync(TerProtocol<IRequest> request)
    {
        
        TerProtocol<DataRequest<string>> blockRequest1 = request.Payload.Data as TerProtocol<DataRequest<string>>;
        
       
        TerProtocol<BlockRequest> blockRequest = request.Payload.Data as TerProtocol<BlockRequest>;

        TerProtocol<DataRequest<string>> blockSimpleRequest = request.Payload.Data as TerProtocol<DataRequest<string>>;

        
        switch (blockRequest.Header.MethodType)
        {
            case MethodType.Get:
                return await _blockService.GetBlocks(blockRequest);
            case MethodType.GetById:
                return await _blockService.GetBlockById(blockSimpleRequest);
            case MethodType.Post:
                return await _blockService.PostBlocks(blockRequest);
            case MethodType.Update:
                return await _blockService.UpdateBlocks(blockRequest);
            case MethodType.Delete:
                return await _blockService.DeleteBlock(blockRequest);
            default:
                var response = new ServerResponseService().GetResponse(false, "Unknown Method.");
                return response;
        }
    }
}