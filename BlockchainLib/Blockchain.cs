using System.Collections.Immutable;
using Utilities;

namespace BlockchainLib
{
    public class Blockchain
    {
        public ImmutableList<Block> Chain { get; private set; }

        public Blockchain()
        {
            Chain = ImmutableList<Block>.Empty;
        }

        public async Task AddBlockAsync(Block block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            Chain = Chain.Add(block);

            await Task.CompletedTask;
        }

        public async Task<Block> GetLastBlockAsync()
        {
            if (Chain.IsEmpty)
                return await Task.FromResult<Block>(null);

            return await Task.FromResult(Chain[^1]);
        }

        public async Task<string> GetLastBlockHashAsync()
        {
            string hash = await Task.FromResult(Chain[^1].Hash);
            return hash;
        }

        public async Task<int> GetCountAsync()
        {
            return Chain.Count;
        }

        

    }
}