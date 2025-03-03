using ModelsLib;

namespace BlockchainLib.Validator;

public interface IValidator
{
   Task<bool> Validate(IModel model);
}