using RRLib.Responses;
using Utilities;

namespace BlockchainLib.Addresses
{
    /// <summary>
    /// The AddressService class handles address management operations, including generating new addresses and retrieving balances for a given address.
    /// </summary>
    public class AddressService
    {
        private AddressManager _AddressManager;  // Instance of AddressManager to handle address-related operations
        private ServerResponseService _serverResponseService; // Instance of ServerResponseService to handle server responses

        /// <summary>
        /// Initializes a new instance of the AddressService class.
        /// </summary>
        public AddressService()
        {
            _AddressManager = new AddressManager();
            _serverResponseService = new ServerResponseService();
        }

        /// <summary>
        /// Generates a new address and its corresponding private key, logs the result, and returns the address and private key as a response.
        /// </summary>
        /// <returns>A response indicating whether the address generation was successful, along with the generated address and private key.</returns>
        public Response GenerateAddress()
        {
            try
            {
                // Generate the address and private key using the AddressManager
                (string address, string privateKey, string publicKey) = _AddressManager.GenerateAddressWithKeys();
            
                // Create the response data containing the address and private key
                var responseData = new
                {
                    Address = address,
                    PrivateKey = privateKey,
                    PubicKey = publicKey
                };

                // Log the successful generation of the address and private key
                Logger.Log("Address and private key were successfully generated.", LogLevel.Information, Source.Server);

                // Return a successful response with the generated address and private key
                return _serverResponseService.GetResponse(
                    success: true,
                    message: "The address was successfully generated.",
                    data: responseData
                );
            }
            catch (Exception ex)
            {
                // Log any errors that occur during address generation
                Logger.Log($"Error while generating address: {ex.Message}", LogLevel.Error, Source.Server);

                // Return a response indicating an error occurred during the process
                return _serverResponseService.GetResponse(
                    success: false,
                    message: "An error occurred while generating the address.",
                    data: null
                );
            }
        }

        /// <summary>
        /// Retrieves the balance for a given address asynchronously, logs the result, and returns the balance.
        /// </summary>
        /// <param name="address">The address whose balance is to be retrieved.</param>
        /// <returns>A response indicating whether the balance retrieval was successful, along with the balance if successful.</returns>
        public async Task<Response> GetBalance(string address)
        {
            try
            {
                // Validate the address (for example, check if it's null or empty, this can be extended to include address format validation)
                if (string.IsNullOrWhiteSpace(address))
                {
                    Logger.Log("Address is null or empty.", LogLevel.Warning, Source.Server);
                    return _serverResponseService.GetResponse(
                        success: false,
                        message: "Address cannot be null or empty.",
                        data: null
                    );
                }

                // Asynchronously get the balance for the given address
                decimal balance = await _AddressManager.GetBalance(address);

                // Log the retrieved balance for the specified address
                Logger.Log($"Balance for address {address}: {balance}", LogLevel.Information, Source.Server);

                // Return a successful response with the retrieved balance
                return _serverResponseService.GetResponse(
                    success: true,
                    message: "Balance retrieved successfully.",
                    data: balance
                );
            }
            catch (Exception ex)
            {
                // Log any errors that occur during balance retrieval
                Logger.Log($"Error while retrieving balance for address {address}: {ex.Message}", LogLevel.Error, Source.Server);

                // Return a response indicating an error occurred during the balance retrieval
                return _serverResponseService.GetResponse(
                    success: false,
                    message: "An error occurred while retrieving the balance.",
                    data: null
                );
            }
        }
    }
}
