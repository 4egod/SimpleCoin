using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XSC.Tests
{
    using Core;

    [TestClass]
    public class BlockTests
    {
        [TestMethod]
        public void GenesisTest()
        {
            Block actual = Block.CreateGenesisBlock();
            Debug.Print(actual.ToJson());
        }
    }
}
