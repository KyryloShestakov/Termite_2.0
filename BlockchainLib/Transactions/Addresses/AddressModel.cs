namespace BlockchainLib.Addresses;

public class AddressModel
{
    /// <summary>
    /// The unique identifier of the address.
    /// </summary>
    public string Address { get; set; }
        
    /// <summary>
    /// The public key from which the address was derived.
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// The private key associated with the address (temporary storage, not recommended for long-term use).
    /// </summary>
    public string PrivateKey { get; set; }

    /// <summary>
    /// The balance associated with the address.
    /// </summary>
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// The timestamp of when the address was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}