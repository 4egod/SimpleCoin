using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace XSC.Tests
{
    using Core;

    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void GenesisTest()
        {
            Transaction actual = Transaction.CreateGenesisTransaction();
            Debug.Print(actual.ToJson());
        }

        [TestMethod]
        public void SerializationTest()
        {
            Transaction actual = Transaction.CreateGenesisTransaction();
            Debug.Print(actual.ExtractData().ToJson());
        }

        [TestMethod]
        public void SignAndCheckTest()
        {
            List<Quantum> inputs = new List<Quantum>();
            inputs.Add(new Quantum(10.0M));
            Quantum output = new Quantum(7.0M);
            byte[] publicKey = "4543533120000000D225B2E78BEAAFC93673E480102B42F3B2A4D6A62D40C95A42A0C3E7C38B495AB942EB76CDF34001884695B501E4B7B3D24784F59419D7BA397DD4EFA555EDA4".FromHex();
            byte[] privateKey = "4543533220000000D225B2E78BEAAFC93673E480102B42F3B2A4D6A62D40C95A42A0C3E7C38B495AB942EB76CDF34001884695B501E4B7B3D24784F59419D7BA397DD4EFA555EDA4F3488226D6252E6A663B4A9B6C77696D4520C7F5D7AAD597159A9DB951BD09A5".FromHex();

            Transaction tx = new Transaction(publicKey, "AkkCpnFFy8PgDdS5jpE2D8", inputs, output);
            TransactionData data = tx.ExtractData();
            string sdata = data.ToJson();

            string hash = Crypto.CalculateHash(data);
            string wrongHash = Crypto.CalculateHash(tx);
            Assert.AreNotEqual(wrongHash, hash);

            tx.Sign(privateKey);
            Debug.Print("TX:\n" + tx.ToJson());
            Debug.Print("DATA:\n" + sdata);
            Assert.AreEqual(hash, tx.Hash);

            bool right = Crypto.VerifyHash(tx.Hash, tx.Key, tx.Signature);
            Assert.IsTrue(right);

            right = tx.Check();
            Assert.IsTrue(right);
        }
    }
}
