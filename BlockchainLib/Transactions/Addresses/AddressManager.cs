using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ModelsLib.BlockchainLib;
using Newtonsoft.Json;
using StorageLib.DB.SqlLite;
using StorageLib.DB.SqlLite.Services.BlockchainDbServices;
using Utilities;

namespace BlockchainLib.Addresses
{
    /// <summary>
    /// Manages the generation and handling of blockchain addresses.
    /// </summary>
    public class AddressManager : IAddressManager
    {
        private BlocksBdService _blocksService;
        public AddressManager()
        {
            _blocksService = new BlocksBdService(new AppDbContext());
        }

        /// <summary>
        /// Generates a new address along with its corresponding private key.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// - `Address`: The generated blockchain address.
        /// - `PrivateKey`: The corresponding private key in Base64 format.
        /// </returns>
        public (string Address, string PrivateKey) GenerateAddressWithKeys()
        {
            using (var rsa = RSA.Create())
            {
                // Export the private key
                var privateKeyBytes = rsa.ExportRSAPrivateKey();
                var privateKeyBase64 = Convert.ToBase64String(privateKeyBytes);

                // Export the public key
                var publicKeyBytes = rsa.ExportRSAPublicKey();

                // Create the address based on the public key
                using (var sha256 = SHA256.Create())
                {
                    var sha256Hash = sha256.ComputeHash(publicKeyBytes);
                    var hash160 = sha256.ComputeHash(sha256Hash);
                    var checksum = ComputeChecksum(hash160);
                    var addressBytes = Combine(hash160, checksum);
                    var address = EncodeBase58(addressBytes);

                    // Return the address and private key
                    return (address, privateKeyBase64);
                }
            }
        }

        /// <summary>
        /// Gets the balance for the specified address.
        /// </summary>
        /// <param name="address">The blockchain address.</param>
        /// <returns>The balance associated with the address.</returns>
        public async Task<decimal> GetBalance(string address)
        {
            decimal incomingAmount = 0;
            decimal outgoingAmount = 0;

            var blocks = await _blocksService.GetAllBlocksAsync();

            foreach (var block in blocks)
            {
                var transactions = GetTransactionsModelFromBlock(block);

                foreach (var transaction in transactions)
                {
                    if (transaction.Receiver == address)
                    {
                        incomingAmount += transaction.Amount;
                    }

                    if (transaction.Sender == address)
                    {
                        outgoingAmount += transaction.Amount;
                    }
                }
            }

            return incomingAmount - outgoingAmount;
        }
        
        public List<TransactionModel> GetTransactionsModelFromBlock(BlockModel block)
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

        /// <summary>
        /// Computes the checksum for the given data using double SHA-256 hashing.
        /// </summary>
        /// <param name="data">The data to compute the checksum for.</param>
        /// <returns>A 4-byte checksum.</returns>
        private byte[] ComputeChecksum(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(data);
                var doubleHash = sha256.ComputeHash(hash);
                return doubleHash[..4];
            }
        }

        /// <summary>
        /// Combines the hash and checksum into a single byte array.
        /// </summary>
        /// <param name="hash">The hash data.</param>
        /// <param name="checksum">The checksum to append.</param>
        /// <returns>A combined byte array of hash and checksum.</returns>
        private byte[] Combine(byte[] hash, byte[] checksum)
        {
            var result = new byte[hash.Length + checksum.Length];
            Buffer.BlockCopy(hash, 0, result, 0, hash.Length);
            Buffer.BlockCopy(checksum, 0, result, hash.Length, checksum.Length);
            return result;
        }

        /// <summary>
        /// Encodes a byte array into a Base58 string.
        /// </summary>
        /// <param name="data">The byte array to encode.</param>
        /// <returns>The Base58-encoded string.</returns>
        private string EncodeBase58(byte[] data)
        {
            const string alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            var sb = new StringBuilder();
            var value = new BigInteger(data.Reverse().Concat(new byte[] { 0 }).ToArray());
            while (value > 0)
            {
                var remainder = (int)(value % 58);
                value /= 58;
                sb.Insert(0, alphabet[remainder]);
            }

            // Add leading zeros as '1'
            foreach (var b in data)
            {
                if (b == 0) sb.Insert(0, '1');
                else break;
            }

            return sb.ToString();
        }
    }
}
