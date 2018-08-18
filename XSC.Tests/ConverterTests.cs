using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XSC.Tests
{
    using Newtonsoft.Json;
    using System.Diagnostics;

    public class TestClass
    {
        public uint Id { get; set; } = 1;

        public string Hash { get; set; } = Crypto.GetRandomHash();

        [JsonConverter(typeof(HexJsonConverter))]
        public byte[] Bytes { get; set; } = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
    }

    [TestClass]
    public class ConverterTests
    {
        private TestClass m_test = new TestClass();

        [TestMethod]
        public void HexTest()
        {
            string s = m_test.Bytes.ToHex();
            Debug.Print(s);
            var actual = s.FromHex();
            Assert.AreEqual("000102030405060708090A0B0C0D0E0F10", s);
            Assert.AreEqual(1, actual[1]);
            Assert.AreEqual(16, actual[16]);
        }

        [TestMethod]
        public void JsonTest()
        {
            string s = m_test.ToJson();
            Debug.Print(s);
            var actual = s.FromJson<TestClass>();
            Assert.AreEqual(m_test.Id, actual.Id);
            Assert.AreEqual(m_test.Hash, actual.Hash);
            Assert.AreEqual(m_test.Bytes.ToHex(), actual.Bytes.ToHex());
        }

        [TestMethod]
        public void Base58Test()
        {
            string s = m_test.Bytes.ToBase58();
            Debug.Print(s);
            var actual = s.FromBase58();
            Assert.AreEqual("18DfbjXLth7APvt3qQPgtf", s);
            Assert.AreEqual(1, actual[1]);
            Assert.AreEqual(16, actual[16]);
        }
    }
}
