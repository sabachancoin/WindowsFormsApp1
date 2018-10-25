using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAll().Wait();
        }

        public static async Task RunAll()
        {
            var node1 = new ClientNode("node1");
            var node2 = new ClientNode("node2");
            var node3 = new ClientNode("node3");
            var node4 = new ClientNode("node4");
            var node5 = new ClientNode("node5");
            var node6 = new ClientNode("node6");

            node1.CreateGenesisBlock();

            node1.Connect(node2);
            node2.Connect(node3);
            node3.Connect(node4);
            node4.Connect(node5);
            node5.Connect(node6);

            node1.Connect(node3);
            node2.Connect(node4);
            node3.Connect(node5);
            node4.Connect(node6);

            await Task.WhenAll(
                node1.Run(logging: true),
                node2.Run(),
                node3.Run(),
                node4.Run(),
                node5.Run(),
                node6.Run());
        }
    }

    public class ClientNode
    {
        private BlockChain localChain = new BlockChain();
        private List<ClientNode> connectedNodes = new List<ClientNode>();
        private readonly string name;
        private int counter;

        public ClientNode(string name)
        {
            this.name = name;
        }

        public void CreateGenesisBlock()
        {
            localChain.CreateGenesis();
        }

        public void OnReceiveChain(BlockChain remoteChain)
        {
            if (!remoteChain.Validate())
            {
                Console.WriteLine("invalid chain received");
                return;
            }

            if(localChain.GetLength() > remoteChain.GetLength())
            {
                Console.WriteLine("smaller length chain received");
                return;
            }

            localChain = remoteChain;
        }

        public void Connect(ClientNode node)
        {
            connectedNodes.Add(node);
        }

        public async Task Run(bool logging = false)
        {
            var random = new Random();

            while (true)
            {
                foreach(var node in connectedNodes)
                {
                    node.OnReceiveChain(localChain);
                }

                await Task.Delay(1000);

                var i = random.Next(1, 6);
                if (i == 5)
                {
                    counter++;

                    if (localChain.TryAddBlock(name + "-" + counter))
                    {
                        Console.WriteLine(name + " added block " + localChain.GetLatestBlock());
                    }

                    if (logging)
                    {
                        Console.WriteLine(localChain);
                    }
                }
            }
        }
    }

    public class BlockChain
    {
        private readonly ReaderWriterLockSlim chainsLock = new ReaderWriterLockSlim();
        private readonly List<Block> chain = new List<Block>();

        public Block GetLatestBlock()
        {
            chainsLock.EnterReadLock();
            try
            {
                return chain[chain.Count - 1];
            }
            finally
            {
                chainsLock.ExitReadLock();
            }
        }

        public int GetLength()
        {
            chainsLock.EnterReadLock();
            try
            {
                return chain.Count;
            }
            finally
            {
                chainsLock.ExitReadLock();
            }
        }

        public void CreateGenesis()
        {
            chainsLock.EnterWriteLock();
            try
            {
                chain.Add(Block.Genesis);
            }
            finally
            {
                chainsLock.ExitWriteLock();
            }
        }

        public bool TryAddBlock(string data = null)
        {
            chainsLock.EnterUpgradeableReadLock();
            try
            {
                if(chain.Count == 0)
                {
                    return false;
                }

                var previousBlock = GetLatestBlock();
                var nextBlock = new Block
                {
                    Index = previousBlock.Index + 1,
                    PreviousHash = previousBlock.Hash,
                    Timestamp = DateTime.UtcNow.ToUnixTime(),
                    Data = data,
                };
                nextBlock.AppendHash();
                if (previousBlock.Validate(nextBlock))
                {
                    chainsLock.EnterWriteLock();
                    try
                    {
                        chain.Add(nextBlock);
                    }
                    finally
                    {
                        chainsLock.ExitWriteLock();
                    }
                    return true;
                }
            }
            finally
            {
                chainsLock.ExitUpgradeableReadLock();
            }

            return false;
        }

        public bool Validate()
        {
            chainsLock.EnterReadLock();
            try
            {
                if(chain.Count == 0)
                {
                    return false;
                }
                if (!Block.Genesis.Equals(chain[0]))
                {
                    return false;
                }
                if (chain.Count == 1)
                {
                    return true;
                }

                for(var i = 1; i < chain.Count - 1; i++)
                {
                    var block = chain[i];
                    if(block == null)
                    {
                        return false;
                    }
                    if(chain[i].PreviousHash != chain[i - 1].Hash)
                    {
                        return false;
                    }
                }
            }
            finally
            {
                chainsLock.ExitReadLock();
            }

            return true;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine();
            builder.AppendLine("===========================");

            chainsLock.EnterReadLock();
            try
            {
                foreach(var block in chain)
                {
                    builder.AppendLine(block.ToString());
                }
            }
            finally
            {
                chainsLock.ExitReadLock();
            }

            builder.AppendLine("===========================");
            builder.AppendLine();
            return builder.ToString();
        }
    }

    public class Block : IEquatable<Block>
    {
        public int Index { get; set; }
        public string PreviousHash { get; set; }
        public long Timestamp { get; set; }
        public string Data { get; set; }
        public string Hash { get; set; }

        public static readonly Block Genesis = new Block
        {
            Index = 0,
            PreviousHash = "0",
            Timestamp = 1512035841,
            Data = "genesis",
            Hash = "c5cd6055557f80eaf2ab3809d1e544d0158198acdd16b80bfa259bfaf521399f",
        };

        public string CalculateHash()
        {
            using(var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(Index + PreviousHash + Timestamp + Data)).ToHexString();
            }
        }

        public void AppendHash()
        {
            Hash = CalculateHash();
        }

        public bool Validate(Block newBlock)
        {
            if (newBlock == null)
            {
                Console.WriteLine("block is null");
                return false;
            }
            if (Index + 1 != newBlock.Index)
            {
                Console.WriteLine("invalid index");
                return false;
            }
            if (Hash != newBlock.PreviousHash)
            {
                Console.WriteLine("invalid previous hash");
                return false;
            }

            var hash = newBlock.CalculateHash();
            if(hash != newBlock.Hash)
            {
                Console.WriteLine("invalid hash: " + hash + " " + newBlock.Hash);
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Index;
                hash = hash * 23 + PreviousHash?.GetHashCode() ?? 0;
                hash = hash * 23 + Timestamp.GetHashCode();
                hash = hash * 23 + Data?.GetHashCode() ?? 0;
                hash = hash * 23 + Hash?.GetHashCode() ?? 0;

                return hash;
            }
        }

        public bool Equals(Block other)
        {
            if (other == null)
                return false;

            return
                Index == other.Index &&
                PreviousHash == other.PreviousHash &&
                Timestamp == other.Timestamp &&
                Data == other.Data &&
                Hash == other.Hash;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Block);
        }

        public override string ToString()
        {
            return $"Index: {Index}, Hash: {Hash}, Data: {Data}";
        }
    }

    public static class Extensions
    {
        private static readonly DateTime UnixEpochDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static long ToUnixTime(this DateTime datetime)
        {
            return (long)datetime.ToUniversalTime().Subtract(UnixEpochDateTime).TotalSeconds;
        }

        internal static string ToHexString(this IReadOnlyCollection<byte> bytes)
        {
            var sb = new StringBuilder(bytes.Count * 2);
            foreach(var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
