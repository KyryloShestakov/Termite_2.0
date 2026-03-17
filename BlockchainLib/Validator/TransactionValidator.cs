using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BlockchainLib.Addresses;
using ModelsLib;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using RRLib.Responses;
using Utilities;

namespace BlockchainLib.Validator;

public class TransactionValidator : IValidator
{
    private ServerResponseService _serverResponseService;

    public TransactionValidator()
    {
        _serverResponseService = new ServerResponseService();
    }

    public async Task<Response> Validate(IModel model)
    {
        if (model is not TransactionModel transaction) return _serverResponseService.GetResponse(false, "Invalid model");

        if (string.IsNullOrEmpty(transaction.Id) || string.IsNullOrEmpty(transaction.Sender) ||
            string.IsNullOrEmpty(transaction.Receiver) || transaction.Amount <= 0 ||
            string.IsNullOrEmpty(transaction.Signature)) return _serverResponseService.GetResponse(false, "Invalid data");

        bool isValidBalance = await VerifyBalance(transaction);
        if (!isValidBalance) return _serverResponseService.GetResponse(false, "Invalid balance");

        bool isValidSignature = VerifySignature(transaction);
        if (!isValidSignature) return _serverResponseService.GetResponse(false, "Invalid signature");
        
        return _serverResponseService.GetResponse(true, "Valid");
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
            transaction.Id, 
            transaction.Sender,
            transaction.Receiver,
            // transaction.Amount,
            // transaction.Timestamp,
            // transaction.Fee,
            // transaction.Data,
            // transaction.Contract,
            // transaction.BlockId,
            // transaction.PublicKey,
        });

        Logger.Log($"Transaction Data for Hashing: {transactionData}", LogLevel.Information, Source.API);
        Logger.Log($"Transaction Signature: {transaction.Signature}", LogLevel.Information, Source.API);

        byte[] transactionHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(transactionData));
        
        Logger.Log($"Transaction Hash: {BitConverter.ToString(transactionHash)}", LogLevel.Information, Source.API);
        try
        {
            using (RSA publicKeyRsa = GetPublicKeyFromString(transaction.PublicKey))
            {
                bool isValid = publicKeyRsa.VerifyData(transactionHash, Convert.FromBase64String(transaction.Signature),
                    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                return isValid;
            }
        }
        catch (CryptographicException ex)
        {
            Logger.Log($"CryptographicException: {ex.Message}", LogLevel.Error, Source.API);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Log($"Unexpected error: {ex.Message}", LogLevel.Error, Source.API);
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

    private RSA GetPublicKeyFromString(string publicKeyBase64)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicKeyBase64))
            {
                throw new ArgumentException("Public key string is null or empty.");
            }

            byte[] keyBytes = Convert.FromBase64String(publicKeyBase64);

            var rsa = RSA.Create();  // Убираем using
            rsa.ImportRSAPublicKey(keyBytes, out _);
        
            return rsa;  // Возвращаем RSA, не уничтожая его внутри метода
        }
        catch (FormatException ex)
        {
            Logger.Log($"Invalid Base64 string: {publicKeyBase64}. Error: {ex.Message}", LogLevel.Error, Source.API);
            throw;
        }
        catch (CryptographicException ex)
        {
            Logger.Log("Failed to import RSA public key due to cryptographic error.", LogLevel.Error, Source.API);
            throw new ArgumentException("Failed to import RSA public key.", ex);
        }
    }

    

    
    private bool IsValidBase64(string input)
    {
        Logger.Log($"Publickey in validatot isValid input: {input}", LogLevel.Warning, Source.Validator);
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();

        if (input.Length % 4 != 0)
        {
            Logger.Log($"Base64 publicKey is not % 4", LogLevel.Warning, Source.Validator);
            return false;
        }
        

        

        return Regex.IsMatch(input, @"^[A-Za-z0-9+/]*={0,2}$");
    }


    private RSA GetPrivateKeyFromString(string privateKey)
    {
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(privateKey.ToCharArray());
        return rsa;
    }
    
}
    
    
