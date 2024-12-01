using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Utilities;

namespace SecurityLib.Security
{
    public class SecureConnectionManager
    {
        private RSA _privateKey;
        private RSAParameters _publicKey;
        private byte[] _sessionKey;

        /// <summary>
        /// Initializes the SecureConnectionManager by generating a new RSA key pair.
        /// </summary>
        public SecureConnectionManager()
        {
            try
            {
                _privateKey = RSA.Create();  // Create a new RSA key pair
                _publicKey = _privateKey.ExportParameters(false);  // Export public key
                Logger.Log("SecureConnectionManager initialized successfully.", LogLevel.Information, Source.Secure);
            }
            catch (Exception ex)
            {
                // Log the error if the initialization fails
                Logger.Log($"Error initializing SecureConnectionManager: {ex.Message}", LogLevel.Error, Source.Secure);
            }
        }

        /// <summary>
        /// Gets the public key used for encryption.
        /// </summary>
        public RSAParameters PublicKey => _publicKey;

        /// <summary>
        /// Exports the RSA public key as a byte array.
        /// </summary>
        /// <returns>Byte array representing the public key.</returns>
        public byte[] GetPublicKeyBytes()
        {
            try
            {
                return _privateKey.ExportRSAPublicKey();  // Export public key as byte array
            }
            catch (Exception ex)
            {
                // Log error if the public key cannot be exported
                Logger.Log($"Error exporting public key: {ex.Message}", LogLevel.Error, Source.Secure);
                throw;
            }
        }

        /// <summary>
        /// Generates a new session key using AES encryption.
        /// </summary>
        /// <returns>Byte array representing the session key.</returns>
        public byte[] GenerateSessionKey()
        {
            try
            {
                using (var aes = Aes.Create())  // Create AES instance
                {
                    aes.GenerateKey();  // Generate a random session key
                    _sessionKey = aes.Key;  // Save the generated session key
                    Logger.Log("Session key generated successfully.", LogLevel.Information, Source.Secure);
                    return _sessionKey;
                }
            }
            catch (Exception ex)
            {
                // Log error if session key generation fails
                Logger.Log($"Error generating session key: {ex.Message}", LogLevel.Error, Source.Secure);
                throw;
            }
        }

        /// <summary>
        /// Encrypts the session key with the recipient's public RSA key.
        /// </summary>
        /// <param name="sessionKey">The session key to encrypt.</param>
        /// <param name="recipientPublicKey">The recipient's public RSA key used for encryption.</param>
        /// <returns>Encrypted session key as a byte array.</returns>
        public byte[] EncryptSessionKey(byte[] sessionKey, RSAParameters recipientPublicKey)
        {
            try
            {
                using (var rsa = RSA.Create())  // Create RSA instance
                {
                    rsa.ImportParameters(recipientPublicKey);  // Import recipient's public key
                    var encryptedSessionKey = rsa.Encrypt(sessionKey, RSAEncryptionPadding.Pkcs1);  // Encrypt session key
                    Logger.Log("Session key encrypted successfully.", LogLevel.Information, Source.Secure);
                    return encryptedSessionKey;
                }
            }
            catch (Exception ex)
            {
                // Log error if encryption fails
                Logger.Log($"Error encrypting session key: {ex.Message}", LogLevel.Error, Source.Secure);
                throw;
            }
        }

        /// <summary>
        /// Converts a Base64-encoded string to RSA parameters.
        /// </summary>
        /// <param name="base64Key">Base64-encoded string representing the RSA public key.</param>
        /// <returns>RSAParameters object.</returns>
        public static RSAParameters ConvertFromBase64ToRSAParameters(string base64Key)
        {
            try
            {
                byte[] keyBytes = Convert.FromBase64String(base64Key);  // Convert Base64 string to byte array
                using (var rsa = RSA.Create())  // Create RSA instance
                {
                    rsa.ImportRSAPublicKey(keyBytes, out _);  // Import RSA public key from byte array
                    Logger.Log("Base64 key converted to RSA parameters.", LogLevel.Information, Source.Secure);
                    return rsa.ExportParameters(false);  // Export public key parameters
                }
            }
            catch (FormatException)
            {
                // Log error if the key is not a valid Base64 string
                Logger.Log("The provided key is not a valid Base64 string.", LogLevel.Error, Source.Secure);
                throw new ArgumentException("The provided key is not a valid Base64 string.");
            }
            catch (Exception ex)
            {
                // Log unexpected errors during the conversion process
                Logger.Log($"Unexpected error converting base64 key: {ex.Message}", LogLevel.Error, Source.Secure);
                throw;
            }
        }

        /// <summary>
        /// Decrypts an encrypted session key using the private RSA key.
        /// </summary>
        /// <param name="encryptedSessionKey">The encrypted session key to decrypt.</param>
        /// <returns>The decrypted session key as a byte array.</returns>
        public byte[] DecryptSessionKey(byte[] encryptedSessionKey)
        {
            try
            {
                var decryptedSessionKey = _privateKey.Decrypt(encryptedSessionKey, RSAEncryptionPadding.Pkcs1);  // Decrypt the session key
                Logger.Log("Session key decrypted successfully.", LogLevel.Information, Source.Secure);
                return decryptedSessionKey;
            }
            catch (Exception ex)
            {
                // Log error if decryption fails
                Logger.Log($"Error decrypting session key: {ex.Message}", LogLevel.Error, Source.Secure);
                throw;
            }
        }

        /// <summary>
        /// Encrypts a message using AES encryption and the provided session key.
        /// </summary>
        /// <param name="message">The message to encrypt.</param>
        /// <param name="sessionKey">The session key used for encryption.</param>
        /// <returns>Encrypted message as a byte array.</returns>
        public byte[] EncryptMessage(string message, byte[] sessionKey)
        {
            try
            {
                using (var aes = Aes.Create())  // Create AES instance
                {
                    aes.Key = sessionKey;  // Set session key
                    aes.GenerateIV();  // Generate random IV
                    using (var encryptor = aes.CreateEncryptor())  // Create AES encryptor
                    {
                        using (var ms = new MemoryStream())  // Create memory stream for encrypted data
                        {
                            ms.Write(aes.IV, 0, aes.IV.Length);  // Write IV to stream
                            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))  // Create CryptoStream for encryption
                            {
                                using (var sw = new StreamWriter(cs))
                                {
                                    sw.Write(message);  // Write message to stream
                                }
                            }
                            Logger.Log("Message encrypted successfully.", LogLevel.Information, Source.Secure);
                            return ms.ToArray();  // Return encrypted message as byte array
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if encryption fails
                Logger.Log($"Error encrypting message: {ex.Message}", LogLevel.Error, Source.Secure);
                throw;
            }
        }

        /// <summary>
        /// Decrypts an encrypted message using AES decryption and the provided session key.
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message to decrypt.</param>
        /// <param name="sessionKey">The session key used for decryption.</param>
        /// <returns>The decrypted message as a string.</returns>
        public string DecryptMessage(byte[] encryptedMessage, byte[] sessionKey)
        {
            try
            {
                using (var aes = Aes.Create())  // Create AES instance
                {
                    using (var ms = new MemoryStream(encryptedMessage))  // Read encrypted message into memory stream
                    {
                        byte[] iv = new byte[16];  // Buffer for IV
                        ms.Read(iv, 0, iv.Length);  // Read IV from stream

                        if (sessionKey.Length != 32)  // Check for valid session key size
                        {
                            Logger.Log("Invalid session key size. Expected 256-bit key.", LogLevel.Error, Source.Secure);
                            throw new ArgumentException("Invalid session key size.");
                        }

                        aes.Key = sessionKey;  // Set session key
                        aes.IV = iv;  // Set IV

                        using (var decryptor = aes.CreateDecryptor())  // Create AES decryptor
                        {
                            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))  // Create CryptoStream for decryption
                            {
                                using (var sr = new StreamReader(cs))  // Read decrypted data
                                {
                                    string decryptedMessage = sr.ReadToEnd();  // Read decrypted message as string
                                    Logger.Log("Message decrypted successfully.", LogLevel.Information, Source.Secure);
                                    return decryptedMessage;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if decryption fails
                Logger.Log($"Error decrypting message: {ex.Message}", LogLevel.Error, Source.Secure);
                throw;
            }
        }
    }
}
