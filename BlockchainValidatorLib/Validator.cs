using BlockchainLib;
using ModelsLib.BlockchainLib;
using Utilities;

namespace BlockchainValidatorLib
{
    /// <summary>
    /// The Validator class is responsible for validating blocks and transactions within the blockchain.
    /// It ensures the integrity of blocks by verifying their previous hashes, Merkle roots, block hashes, 
    /// and the validity of each transaction contained within the block.
    /// </summary>
    public class Validator
    {
        private MerkleTree _merkleTree;  // Instance of MerkleTree to compute and validate Merkle root.
        private TransactionManager _transactionManager;  // Instance of TransactionManager for managing and validating transactions.

        /// <summary>
        /// Initializes the Validator class by creating instances of MerkleTree and TransactionManager.
        /// </summary>
        public Validator()
        {
            _merkleTree = new MerkleTree();
            _transactionManager = new TransactionManager();
        }

        /// <summary>
        /// Validates a block by ensuring its integrity and checking that its previous hash, Merkle root, 
        /// block hash, and transactions are all valid.
        /// </summary>
        /// <param name="block">The block to validate.</param>
        /// <param name="expectedPreviousHash">The expected hash of the previous block in the chain.</param>
        /// <returns>Returns true if the block is valid; false otherwise.</returns>
        public async Task<bool> ValidateBlock(Block block, string expectedPreviousHash)
        {
            // Validate the previous hash by comparing it with the expected previous hash in the chain
            if (block.PreviousHash != expectedPreviousHash)
            {
                Logger.Log("The previous hash is incorrect.", LogLevel.Warning, Source.Validator);
                return false;
            }

            // Validate the Merkle root by calculating and comparing it
            List<Transaction> transactions = block.Transactions;
            string calculatedMerkleRoot = _merkleTree.CalculateMerkleRoot(transactions);
            if (block.MerkleRoot != calculatedMerkleRoot)
            {
                Logger.Log("The merkle root is incorrect.", LogLevel.Warning, Source.Validator);
                return false;
            }

            // Validate the block hash by generating a hash and comparing it
            string calculatedHash = block.GenerateHash();
            if (block.Hash != calculatedHash)
            {
                Console.WriteLine("The block hash is incorrect.");
                return false;
            }

            // Validate each transaction contained within the block
            foreach (var transaction in block.Transactions)
            {
                if (!ValidateTransaction(transaction, transaction.Sender))
                {
                    Console.WriteLine("One or more transactions are invalid.");
                    return false;
                }
            }

            Console.WriteLine("The block is valid.");
            return true;
        }

        /// <summary>
        /// Validates a transaction by checking its signature and verifying that the sender is legitimate.
        /// </summary>
        /// <param name="transaction">The transaction to validate.</param>
        /// <param name="senderPublicKey">The public key of the sender to verify the transaction's signature.</param>
        /// <returns>Returns true if the transaction's signature is valid; false otherwise.</returns>
        private static bool ValidateTransaction(Transaction transaction, string senderPublicKey)
        {
            try
            {
                // Prepare the data to be signed for the transaction's validation
                string dataToSign = $"{transaction.Sender}{transaction.Receiver}{transaction.Amount}{transaction.Fee}{transaction.Timestamp}";

                // If additional data or contract are present, append them to the data to be signed
                if (transaction.Data != null)
                {
                    dataToSign += transaction.Data.ToString();
                }

                if (transaction.Contract != null)
                {
                    dataToSign += transaction.Contract.ToString();
                }

                // Create an RSA object to verify the transaction's signature using the sender's public key
                using (var rsa = System.Security.Cryptography.RSA.Create())
                {
                    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(senderPublicKey), out _);

                    // Verify the signature using the sender's public key, SHA256, and RSA PKCS1 padding
                    bool isSignatureValid = rsa.VerifyData(
                        System.Text.Encoding.UTF8.GetBytes(dataToSign),
                        Convert.FromBase64String(transaction.Signature),
                        System.Security.Cryptography.HashAlgorithmName.SHA256,
                        System.Security.Cryptography.RSASignaturePadding.Pkcs1
                    );

                    return isSignatureValid;
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during transaction validation
                Logger.Log($"Error validating transaction: {ex.Message}", LogLevel.Error, Source.App);
                return false;
            }
        }
    }
}
