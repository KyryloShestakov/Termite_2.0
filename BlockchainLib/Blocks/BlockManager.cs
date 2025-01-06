using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
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
            // Преобразуем хэш блока в массив байтов
            byte[] hashBytes = Convert.FromBase64String(generateHash);

            // Используем RSA для подписания хэша
            byte[] signatureBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                // Подписываем хэш
                signatureBytes = _privateKey.SignHash(hashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }

            // Возвращаем подпись в Base64 строковом формате
            return Convert.ToBase64String(signatureBytes);
        }
        catch (Exception ex)
        {
            // Логируем ошибку
            Logger.Log($"Error in SignBlock: {ex.Message}", LogLevel.Error, Source.Blockchain);
            throw;
        }
    }
}