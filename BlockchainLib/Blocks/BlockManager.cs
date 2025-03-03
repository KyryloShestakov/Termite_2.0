using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using Utilities;
using LogLevel = Utilities.LogLevel;

namespace BlockchainLib;

public class BlockManager
{
    private readonly RSA _privateKey;

    public BlockManager()
    {
        _privateKey = RSA.Create();
    }
    
    public string SignBlock(string generateHash)
    {
        try
        {
            if (string.IsNullOrEmpty(generateHash))
            {
                Logger.Log($"Could't sign empty block : {generateHash}", LogLevel.Error, Source.Blockchain);
            }

            byte[] hashBytes = Convert.FromBase64String(generateHash);

            byte[] signatureBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                signatureBytes = _privateKey.SignHash(hashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }

            return Convert.ToBase64String(signatureBytes);
        }
        catch (Exception ex)
        {
            Logger.Log($"Error in SignBlock: {ex.Message}", LogLevel.Error, Source.Blockchain);
            throw;
        }
    }
    
    public List<TransactionModel> GetTransactionsFromBlock(BlockModel block)
    {
        var transactions = new List<TransactionModel>();

        if (!string.IsNullOrEmpty(block.Transactions))
        {
            try
            {
                transactions = JsonConvert.DeserializeObject<List<TransactionModel>>(block.Transactions);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deserializing transactions: {ex.Message}", LogLevel.Error, Source.App);
            }
        }

        return transactions;
    }
}