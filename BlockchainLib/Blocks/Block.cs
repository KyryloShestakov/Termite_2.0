using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ModelsLib.BlockchainLib;
using Utilities;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BlockchainLib
{
    /// <summary>
    /// Represents a block in the blockchain containing transactions and associated metadata.
    /// Includes methods for hash generation, size calculation, and conversion between models.
    /// </summary>
    public class Block
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string MerkleRoot { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Difficulty { get; set; }
        public string Nonce { get; set; }
        public string Signature { get; set; }
        public int Size { get; set; }

        /// <summary>
        /// Constructor to create a new block with specified parameters.
        /// </summary>
        public Block(int index, List<Transaction> transactions, string previousHash, string merkleRoot, int difficulty, string nonce, string signature)
        {
            try
            {
                Id = Guid.NewGuid().ToString();
                Index = index;
                Timestamp = DateTime.UtcNow;
                Transactions = transactions;
                PreviousHash = previousHash;
                MerkleRoot = merkleRoot;
                Difficulty = difficulty;
                Nonce = nonce;
                Signature = signature;
              //  Hash = GenerateHash();
              //  Size = CalculateSize();

                Logger.Log("Block created successfully.", LogLevel.Information, Source.Blockchain);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during block initialization: {ex.Message}", LogLevel.Error, Source.Blockchain);
            }
        }

        public Block()
        {
            try
            {
                Timestamp = DateTime.UtcNow;
              //  Hash = GenerateHash();
              //  Size = CalculateSize();

               // Logger.Log("Empty block created successfully.", LogLevel.Information, Source.Blockchain);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during empty block creation: {ex.Message}", LogLevel.Error, Source.Blockchain);
            }
        }

        /// <summary>
        /// Generates the hash for the block based on its properties.
        /// </summary>
        /// <returns>A string representing the hash of the block.</returns>
        public string GenerateHash()
        {
            try
            {
                string blockData = $"{Index}{Timestamp}{PreviousHash}{MerkleRoot}{Nonce}";
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
                    Logger.Log($"Hash: {Convert.ToBase64String(hashBytes)}", LogLevel.Information, Source.Blockchain);
                    Logger.Log("Hash generated successfully.", LogLevel.Information, Source.Blockchain);
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during hash generation: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

        /// <summary>
        /// Calculates the size of the block in bytes.
        /// </summary>
        /// <returns>Size of the block in bytes.</returns>
        public int CalculateSize()
        {
            try
            {
                int size = sizeof(int) + Hash.Length + PreviousHash.Length + MerkleRoot.Length +
                           sizeof(long) + sizeof(int) + Nonce.Length + Signature.Length;

                foreach (var transaction in Transactions)
                {
                    size += transaction.CalculateSize();
                }

                Logger.Log("Block size calculated successfully.", LogLevel.Information, Source.Blockchain);
                return size;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during size calculation: {ex.Message}", LogLevel.Error, Source.Blockchain);
                return 0;
            }
        }

        /// <summary>
        /// Converts the current block to a BlockModel instance.
        /// </summary>
        public BlockModel ToBlockModel()
        {
            try
            {
                string transactions = JsonConvert.SerializeObject(Transactions);

                var model = new BlockModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Index = Index,
                    Timestamp = Timestamp,
                    Transactions = transactions,
                    MerkleRoot = MerkleRoot,
                    PreviousHash = PreviousHash,
                    Hash = Hash,
                    Difficulty = Difficulty,
                    Nonce = Nonce,
                    Signature = Signature,
                    Size = Size
                };

                Logger.Log("Block converted to BlockModel successfully.", LogLevel.Information, Source.Blockchain);
                return model;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during block-to-model conversion: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

        /// <summary>
        /// Converts a BlockModel instance to a Block instance.
        /// </summary>
        public static Block FromBlockModel(BlockModel model)
        {
            try
            {
                List<Transaction> transactions;
                var transactionsJson = model.Transactions;

                if (transactionsJson.TrimStart().StartsWith("{"))
                {
                    var singleTransaction = JsonConvert.DeserializeObject<Transaction>(transactionsJson);
                    transactions = new List<Transaction> { singleTransaction };
                }
                else
                {
                    transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson);
                }

                var block = new Block
                {
                    Index = model.Index,
                    Timestamp = model.Timestamp,
                    MerkleRoot = model.MerkleRoot,
                    PreviousHash = model.PreviousHash,
                    Hash = model.Hash,
                    Difficulty = model.Difficulty,
                    Nonce = model.Nonce,
                    Signature = model.Signature,
                    Size = model.Size,
                    Transactions = transactions
                };

                Logger.Log("BlockModel converted to Block successfully.", LogLevel.Information, Source.Blockchain);
                return block;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during model-to-block conversion: {ex.Message}", LogLevel.Error, Source.Blockchain);
                throw;
            }
        }

    }
}
