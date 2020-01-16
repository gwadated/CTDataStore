namespace UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Celcat.Verto.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestItemBatcher
    {
        private readonly Random _random = new Random();

        [TestMethod]
        public void CorrectlyRetrieves()
        {
            var ids = GeneratedIds();
            int maxBatch = _random.Next(100, 200);
            var batcher = new ItemBatcher<int>(ids, maxBatch);

            var finalIds = new HashSet<int>();

            var batch = batcher.GetBatch();
            while (batch != null)
            {
                Assert.IsTrue(batch.Any());
                Assert.IsTrue(batch.Count() <= maxBatch);

                finalIds.UnionWith(batch);

                batch = batcher.GetBatch();
            }

            Assert.AreEqual(finalIds.Count, ids.Count);
        }

        private List<int> GeneratedIds()
        {
            var result = new List<int>();

            int count = _random.Next(500, 2000);
            for (int n = 0; n < count; ++n)
            {
                result.Add(n);
            }

            return result;
        }
    }
}
