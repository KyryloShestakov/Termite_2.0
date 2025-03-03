using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using DataLib.DB.SqlLite.Interfaces;
using ModelsLib;
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
        private BlockManager _blockManager;
        public AddressManager()
        {
            _blockManager = new BlockManager();
        }

        /// <summary>
        /// Generates a new address along with its corresponding private key.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// - `Address`: The generated blockchain address.
        /// - `PrivateKey`: The corresponding private key in Base64 format.
        /// </returns>
        public (string Address, string PrivateKey, string PublicKey) GenerateAddressWithKeys()
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
                    var publicKey = EncodeBase58(publicKeyBytes);
                    // Return the address and private key
                    return (address, privateKeyBase64, publicKey);
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
            IDbProcessor _dbProcessor = new DbProcessor();
            
            decimal incomingAmount = 0;
            decimal outgoingAmount = 0;

            List<IModel> models = await _dbProcessor.ProcessService<List<IModel>>(new BlocksBdService(new AppDbContext()), CommandType.GetAll);
            List<BlockModel> blocks = models.Cast<BlockModel>().ToList();
            foreach (var block in blocks)
            {
                var transactions = _blockManager.GetTransactionsFromBlock(block);

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
