using System.Security.Cryptography;
using System.Text;
using BlockchainLib.Addresses;
using ModelsLib;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;

namespace BlockchainLib.Validator;

public class TransactionValidator : IValidator
{
    public async Task<bool> Validate(IModel model)
    {
        if (model is not TransactionModel transaction) return false;
        
        if (string.IsNullOrEmpty(transaction.Id) || string.IsNullOrEmpty(transaction.Sender) ||
            string.IsNullOrEmpty(transaction.Receiver) || transaction.Amount <= 0 ||
            string.IsNullOrEmpty(transaction.Signature)) return false;

        bool isValidBalance = await VerifyBalance(transaction);
        if (!isValidBalance) return false;

       bool isValidSignature = VerifySignature(transaction);
       if (!isValidSignature) return false;
       
       return true;
    }
    
    public bool VerifySignature(TransactionModel transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction.PublicKey == null) throw new ArgumentNullException(nameof(transaction.PublicKey));
        if (string.IsNullOrEmpty(transaction.Signature)) return false;
        bool isValidUuid = Guid.TryParse(transaction.Id, out Guid parsedUuid);
        if (!isValidUuid) return false;
        
        string transactionData = JsonConvert.SerializeObject(new
        {
            transaction.Sender,
            transaction.Receiver,
            transaction.Amount,
            transaction.Timestamp,
            transaction.Fee,
            transaction.Data,
            transaction.Contract
        });

        byte[] transactionHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(transactionData));

        RSA publicKeyRsa = GetPublicKeyFromString(transaction.PublicKey);

        try
        {
            return publicKeyRsa.VerifyData(transactionHash, Convert.FromBase64String(transaction.Signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private static async Task<bool> VerifyBalance(TransactionModel transaction)
    {
        AddressManager addressManager = new AddressManager();
        decimal balance = await addressManager.GetBalance(transaction.Sender);
        if (balance < transaction.Amount + transaction.Fee) return false;
        return true;
    }

    private RSA GetPublicKeyFromString(string publicKey)
    {
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKey.ToCharArray());
        return rsa;
    }

    private RSA GetPrivateKeyFromString(string privateKey)
    {
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(privateKey.ToCharArray());
        return rsa;
    }
}
    
    
