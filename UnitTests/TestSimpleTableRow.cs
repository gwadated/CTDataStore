namespace UnitTests
{
    using Celcat.Verto.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class TestSimpleTableRow
    {
        [TestMethod]
        public void CanAddNullValues()
        {
            var row = new SimpleTableRow();
            row.Add(null);

            Assert.IsTrue(row.ColumnCount == 1);
        }
    }
}
