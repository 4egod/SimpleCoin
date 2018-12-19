using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace XSC.Tests
{
    using Core;
    using DB;

    [TestClass]
    public class DatabaseTests
    {
        //private bool create = true;

        private Database db;

        [TestInitialize]
        public void Initialize()
        {
            db = Database.Create("PL1xVuL6jWLsqhnZde6mBq");
            //db = Database.Open("KphYX5KsXKYLXGt6LYvX6Z");
        }

        [TestMethod]
        public void AddBlockTest()
        {
            List<Transaction> transactions = new List<Transaction>();
            Block block = new Block("PL1xVuL6jWLsqhnZde6mBq", Block.CreateGenesisBlock(), transactions, 1);
            block.CalculateHash();
            db.Add(block);
        }

        [TestMethod]
        public void GetHeightTest()
        {
            ulong height = db.Height;
            Debug.Print($"Height: {height}");
        }

        [TestMethod]
        public void GetBlockByHeightTest()
        {
            var block = db.GetBlock(0);
            Debug.Print(block.ToJson());
        }

        [TestMethod]
        public void GetBlockByHashTest()
        {
            var block = db.GetBlock("0000000000000000000000000000000000000000000000000000000000000000");
            Debug.Print(block.ToJson());
        }

        [TestMethod]
        public void GetTransactionTest()
        {
            var transaction = db.GetTransaction("0000000000000000000000000000000000000000000000000000000000000000");
            Debug.Print(transaction.ToJson());
        }
    }
}
