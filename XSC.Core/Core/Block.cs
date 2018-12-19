using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XSC.Core
{
    using DB;
    using LiteDB;
    using Newtonsoft.Json;

    internal class BlockData
    {
        public string PreviousHash { get; set; }

        public ulong Height { get; set; }

        public int Difficulty { get; set; }

        public List<Transaction> Transactions { get; set; }

        public ulong Nonce { get; set; }
    }

    [DataContract]
    public class Block
    {
        [JsonConstructor]
        private Block() { }

        public Block(string minerAddress, Block previousBlock, IList<Transaction> transactions)
        {
            PreviousHash = previousBlock.Hash;
            Height = previousBlock.Height + 1;
            Difficulty = Config.StaticDifficulty;
            Transactions.Add(Transaction.CreateRewardTransaction(minerAddress));
            Transactions.AddRange(transactions);
        }

        public Block(string minerAddress, Block previousBlock, IList<Transaction> transactions, int difficulty)
        {
            PreviousHash = previousBlock.Hash;
            Height = previousBlock.Height + 1;
            Difficulty = difficulty;
            Transactions.Add(Transaction.CreateRewardTransaction(minerAddress));
            Transactions.AddRange(transactions);
        }

        [DataMember]
        [JsonProperty(Required = Required.Always)]
        public string Hash { get; private set; }

        [DataMember]
        [JsonProperty(Required = Required.Always)]
        public string PreviousHash { get; private set; }

        [BsonId]
        [DataMember]
        [JsonProperty(Required = Required.Always)]
        public ulong Height { get; private set; }

        [DataMember]
        [JsonProperty(Required = Required.Always)]
        public DateTime Timestamp { get; set; }

        [DataMember]
        [JsonProperty(Required = Required.Always)]
        public int Difficulty { get; private set; }

        [DataMember]
        [JsonProperty(Required = Required.Always)]
        [BsonRef(Database.TransactionsCollectionName)]
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();

        [DataMember]
        [JsonProperty(Required = Required.Always)]
        public ulong Nonce { get; set; }

        public void CalculateHash()
        {
            Nonce++;
            Hash = Crypto.CalculateHash(ExtractData());
        }

        /// <summary>
        /// Full check
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            foreach (var tx in Transactions)
            {
                if (!tx.Check()) return false;
            }

            if (!CheckHash()) return false;

            return true; 
        }

        /// <summary>
        /// Check only hash
        /// </summary>
        /// <returns></returns>
        public bool CheckHash()
        {
            for (int i = 0; i < Difficulty; i++)
            {
                if (Hash[i] != '0') return false;
            }

            return true;
        }

        public static Block CreateGenesisBlock()
        {
            Block res = new Block();
            res.Hash = Config.GenesisHash;
            res.PreviousHash = Config.GenesisHash;
            res.Transactions.Add(Transaction.CreateGenesisTransaction());

            return res;
        }

        internal BlockData ExtractData()
        {
            BlockData data = new BlockData();
            data.PreviousHash = PreviousHash;
            data.Height = Height;
            data.Difficulty = Difficulty;
            data.Transactions = Transactions;
            data.Nonce = Nonce;

            return data;
        }
    }
}
