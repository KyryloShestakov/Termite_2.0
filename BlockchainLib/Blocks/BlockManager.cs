using Microsoft.Extensions.Logging;
using Utilities;

namespace BlockchainLib;

public class BlockManager
{
    public BlockManager()
    {
    }

    public async Task<Block> CreateNewBlock(int index, List<Transaction> transactions, string previousHash, string merkleRoot, int difficulty, string nonce, string signature)
    {
        try
        {
            // Логируем создание нового блока
            Logger.Log($"Creating new block at index {index} with previous hash: {previousHash}, difficulty: {difficulty}");
            

            // Создаем новый блок с использованием конструктора
            Block newBlock = new Block(index, transactions, previousHash, merkleRoot, difficulty, nonce, signature);

            // Логируем успешное создание блока
            Logger.Log($"New block created successfully at index {index} with hash: {newBlock.Hash}");

            return newBlock;
        }
        catch (Exception ex)
        {
            // Логируем ошибку, если создание блока не удалось
            Logger.Log($"Error while creating new block at index {index}: {ex.Message}");
            throw; // Прокидываем ошибку дальше для дальнейшей обработки
        }
    }



    public string SignBlock(string generateHash)
    {
        throw new NotImplementedException();
    }
}