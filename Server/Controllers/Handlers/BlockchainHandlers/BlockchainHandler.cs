using BlockchainLib;
using RRLib.Responses;
using Ter_Protocol_Lib.Requests;
using Utilities;

namespace Server.Controllers.Handlers.BlockchainHandlers;

public class BlockchainHandler : IRequestHandler
{
    private BlockchainService _blockchainService { get; }

    public BlockchainHandler()
    {
        _blockchainService = new BlockchainService();
    }

    public async Task<Response> HandleRequestAsync(TerProtocol<object> request)
    {
        string json = request.Payload.Serialize();
        Logger.Log(json, LogLevel.Warning, Source.Server);
        var obj = RequestSerializer.DeserializeData(request.Header.MessageType, json);

        InfoSyncRequest txRequest = (InfoSyncRequest)obj;
       
            TerProtocol<object> terProtocol =
                new TerProtocol<object>(request.Header, new TerPayload<object>(txRequest));

            switch (request.Header.MethodType)
            {
                case MethodType.Get:
                    Response txGetResponse = await _blockchainService.GetBlockchainInfo(terProtocol);
                    return txGetResponse;

                default:
                    Logger.Log("Unknown command.", LogLevel.Warning, Source.Server);
                    break;

            }
            
            return new ServerResponseService().GetResponse(true, "Request processed successfully.");

    }
}
