using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BlockchainLib;

public class MerkleTree
{
    public string MerkleRoot { get; private set; }

    public MerkleTree()
    {
    }

    public string CalculateMerkleRoot(List<Transaction> transactions)
    {
        List<string> leaves = new List<string>();
        foreach (var tx in transactions)
        {
            leaves.Add(CalculateHash(SerializeTransaction(tx)));
        }

        string merkleRoot = BuildTree(leaves);
        return merkleRoot;
    }

    private string BuildTree(List<string> nodes)
    {
        if (nodes.Count == 1)
        {
            return nodes[0];
        }

        List<string> parentNodes = new List<string>();

        for (int i = 0; i < nodes.Count; i += 2)
        {
            if (i + 1 < nodes.Count)
            {
                parentNodes.Add(CalculateHash(nodes[i] + nodes[i + 1]));
            }
            else
            {
                parentNodes.Add(nodes[i]);
            }
        }

        return BuildTree(parentNodes);
    }

    private string CalculateHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }

    private string SerializeTransaction(Transaction tx)
    {
        return JsonSerializer.Serialize(tx);
    }
}