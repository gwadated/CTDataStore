namespace UnitTests
{
    using System;
    using Celcat.Verto.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestFlexiDictionary
    {
        private readonly Random _random = new Random();

        [TestMethod]
        public void AllowsDynamicCreationOfKeys()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var fd = new FlexiDictionary();
            var id = fd["foo"];
            Assert.IsNull(id);
        }

        [TestMethod]
        public void CorrectlyWritesAndReads()
        {
            var fd = new FlexiDictionary();

            int value1 = _random.Next();
            fd["foo"] = value1;

            int value2 = (int)fd["foo"];
            Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Duplicate keys should not be allowed")]
        public void DoesntAllowDuplicates()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new FlexiDictionary { { "foo", 1 }, { "foo", 2 } };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Is case sensitive")]
        public void IsNotCaseSensitive()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new FlexiDictionary { { "foo", 1 }, { "FOO", 2 } };
        }
    }
}
