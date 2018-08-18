using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XSC.Tests
{
    using Core;
    using System.Diagnostics;
    using System.IO;

    [TestClass]
    public class WalletTests
    {
        [TestMethod]
        public void SerializationTest()
        {
            Wallet w1 = Wallet.Generate();

            string s = w1.ToJson();
            Debug.Print("W1: " + s);

            string expected = w1.PrivateKey.ToHex();

            Wallet w2 = s.FromJson<Wallet>();
            Debug.Print("W2: " + w2.ToJson());

            string actual = w2.PrivateKey.ToHex();
            Assert.AreEqual(expected, actual);

            expected = w1.PublicKey.ToHex();
            actual = w2.PublicKey.ToHex();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GenerateNewTest()
        {
            Wallet w1 = Wallet.Generate();
            Wallet w2 = Wallet.Generate();
            Debug.Print($"{w1.ToJson()}\n\n{w2.ToJson()}");
            Assert.AreNotEqual(w1.Address, w2.Address);
        }

        [TestMethod]
        public void ImportTest()
        {
            Wallet actual = Wallet.Import(
                ($"4543533220000000D225B2E78BEAAFC93673E480102B42F3B2A4D6A62D40C95A42A0C3E7C38B495AB942EB76CDF34001884695B501E4B7B3D24784F59419D7BA397DD4EFA555EDA4F3488226D6252E6A663B4A9B6C77696D4520C7F5D7AAD597159A9DB951BD09A5")
                .FromHex());
            Debug.Print($"{actual.ToJson()}");
            Assert.AreEqual("AkkCpnFFy8PgDdS5jpE2D8", actual.Address);
        }

        [TestMethod]
        public void ExportImportTest()
        {
            Wallet expected = Wallet.Generate();
            expected.Export();

            Wallet actual = Wallet.Import(expected.Address);
            Assert.AreEqual(expected.ToJson(), actual.ToJson());
        }
    }
}
