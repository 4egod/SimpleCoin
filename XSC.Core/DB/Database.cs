using System.Collections.Generic;
using System.IO;

namespace XSC.DB
{
    using Core;
    using LiteDB;
    using Properties;
    using Logger = XSC.Logger;

    public class Database
    {
        public const string Extension = ".scdb";
        public const string BlocksCollectionName = "Blocks";
        public const string TransactionsCollectionName = "Transactions";
        public const string InputsCollectionName = "Inputs";
        public const string OutputsCollectionName = "Outputs";
        
        private string address;

        internal Database(string address)
        {
            this.address = address;
            GenesisBlock = Block.CreateGenesisBlock();
        }

        public static Database Open(string address)
        {
            Database db = new Database(address);

            if (!File.Exists(db.Path))
            {
                throw new FileNotFoundException(db.Path);
            }

            db.InternalOpen();

            return db;
        }

        public static Database Create(string address)
        {
            Database db = new Database(address);

            if (File.Exists(db.Path))
            {
                File.Delete(db.Path);
            }

            db.InternalCreate();
            db.InternalOpen();

            return db;
        }

        public string Path => Config.DataDirectory + address + Extension;

        public Block GenesisBlock { get; private set; }

        public ulong Height { get; private set; }

        public Block LastBlock { get; private set; }

        public void Add(Block block)
        {
            foreach (var item in block.Transactions)
            {
                item.FromBlock = block.Height;
                Add(item);
            }

            using (var db = new LiteDatabase(Path))
            {
                var blocks = db.GetCollection<Block>(BlocksCollectionName);

                blocks.Insert(block);
            }

            LastBlock = block;
            Height = block.Height;
        }

        public void Add(Transaction transaction)
        {
            using (var db = new LiteDatabase(Path))
            {
                var transactions = db.GetCollection<Transaction>(TransactionsCollectionName);
                transactions.Insert(transaction);

                var inputs = db.GetCollection<Quantum>(InputsCollectionName);
                var outputs = db.GetCollection<Quantum>(OutputsCollectionName);
                var target_inputs = db.GetCollection<Quantum>(transaction.Recipient);

                if (transaction.Reward != null)
                {
                    inputs.Insert(transaction.Reward);
                    target_inputs.Insert(transaction.Reward);
                    return;
                }

                outputs.Insert(transaction.Output);
                target_inputs.Insert(transaction.Output);
                
                var source_inputs = db.GetCollection<Quantum>(Wallet.GetAddress(transaction.Key));

                if (transaction.Change != null)
                {
                    inputs.Insert(transaction.Change);
                    source_inputs.Insert(transaction.Change);
                }
                
                foreach (var item in transaction.Inputs)
                {
                    source_inputs.Delete(x => x.Hash == item.Hash);
                }
            }
        }

        public Block GetBlock(ulong height)
        {
            using (var db = new LiteDatabase(Path))
            {
                var blocks = db.GetCollection<Block>(BlocksCollectionName);
                return blocks.Include(x => x.Transactions).FindOne(x => x.Height == height);
            }
        }

        public Block GetBlock(string hash)
        {
            using (var db = new LiteDatabase(Path))
            {
                var blocks = db.GetCollection<Block>(BlocksCollectionName);
                return blocks.Include(x => x.Transactions).FindOne(x => x.Hash == hash);
            }
        }

        public Transaction GetTransaction(string hash)
        {
            using (var db = new LiteDatabase(Path))
            {
                var transactions = db.GetCollection<Transaction>(TransactionsCollectionName);
                return transactions.FindOne(x => x.Hash == hash);
            }
        }

        public IEnumerable<Quantum> GetInputs()
        {
            using (var db = new LiteDatabase(Path))
            {
                var inputs = db.GetCollection<Quantum>(address);
                return inputs.FindAll();
            }
        }

        public decimal GetBalance()
        {
            return GetBalance(address);
        }

        public decimal GetBalance(string address)
        {
            using (var db = new LiteDatabase(Path))
            {
                var inputs = db.GetCollection<Quantum>(address);

                var values = inputs.FindAll();

                decimal res = 0;

                foreach (var item in values)
                {
                    res += item.Amount / Config.DecimalPoint;
                }

                return res;
            }
        }

        //public ulong GetHeight()
        //{
        //    using (var db = new LiteDatabase(Path))
        //    {
        //        var blocks = db.GetCollection<Block>(BlocksCollectionName);

        //        return (ulong)blocks.Max().AsInt64;
        //    }
        //}

        private void InternalCreate()
        {
            Add(Block.CreateGenesisBlock());
            CreateIndexes();
            Logger.WriteLine(Resources.DbCreated, Path);
        }

        private void InternalOpen()
        {
            using (var db = new LiteDatabase(Path))
            {
                var blocks = db.GetCollection<Block>(BlocksCollectionName);

                //Query query = Query.Where("Hash", x=> true, -1); 
                //var lastBlocks = blocks.Find(Query.All(Query.Descending), 0, 10);

                Height = (ulong)blocks.Max().AsInt64;
                LastBlock = blocks.FindOne(x => x.Height == Height);
            }

            Logger.WriteLine(Resources.DbLoading, Path);
            string balance = GetBalance().ToString(Config.BalanceFormatWithTicker);
            Logger.WriteLine(Resources.DbOpened);
            Logger.WriteLine(Resources.DbInfo, address, balance, Height);
        }

        private void CreateIndexes()
        {
            using (var db = new LiteDatabase(Path))
            {
                var blocks = db.GetCollection<Block>(BlocksCollectionName);
                blocks.EnsureIndex(x => x.Hash);

                var transactions = db.GetCollection<Transaction>(TransactionsCollectionName);
                transactions.EnsureIndex(x => x.Hash);
            }
        }


    }
}
