using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace XSC.Tests
{
    using Core;

    [TestClass]
    public class CryptoTests
    {
        [TestMethod]
        public void GetRandomHashTest()
        {
            var h1 = Crypto.GetRandomHash();
            var h2 = Crypto.GetRandomHash();
            Assert.AreNotEqual(h1, h2);
            Debug.Print($"{h1}\n{h2}");
        }

        [TestMethod]
        public void MiningTest()
        {
            List<Transaction> transactions = new List<Transaction>();
            Block block = new Block("PL1xVuL6jWLsqhnZde6mBq", Block.CreateGenesisBlock(), transactions, 1);
            
            Crypto.CalculateHash(block);
            Debug.Print(block.ToJson());        
        }
    }
}
