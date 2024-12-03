namespace BlockchainLib;

public interface IBlockManager
{
    Block CreateBlock(string previousBlockHash, List<Transaction> transactions);
    bool MineBlock(Block block, int difficulty);
    void AddBlockToChain(Block block);
    List<Block> GetChain();
    bool VerifyChain();

}