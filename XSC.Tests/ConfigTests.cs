using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XSC.Tests
{
    using Core;

    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void GetBaseDirectoryTest()
        {
            string actual = Config.BaseDirectory;
            Assert.AreEqual(actual[actual.Length - 1], '\\');
        }
    }
}
