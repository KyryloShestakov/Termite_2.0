using ModelsLib;
using RRLib.Responses;

namespace BlockchainLib.Validator;

public interface IValidator
{
   Task<Response> Validate(IModel model);
}