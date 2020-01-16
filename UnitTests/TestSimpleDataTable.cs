namespace UnitTests
{
    using System;
    using System.Collections.Generic;
    using Celcat.Verto.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestSimpleDataTable
    {
        private readonly Random _random = new Random();

        [TestMethod]
        public void StorageWorks()
        {
            var t = new SimpleTableData();

            int numCols = _random.Next(1, 50);

            for (int n = 0; n < numCols; ++n)
            {
                t.AddColumn(Guid.NewGuid().ToString());
            }

            var rows = new List<SimpleTableRow>();

            int numRows = _random.Next(100, 1000);
            for (int n = 0; n < numRows; ++n)
            {
                var row = new SimpleTableRow();

                for (int c = 0; c < numCols; ++c)
                {
                    row.Add(Guid.NewGuid().ToString());
                }

                rows.Add(row);
                t.AddRow(row);
            }

            int i = 0;
            foreach (var row in t.Rows)
            {
                Assert.AreEqual(row, rows[i++]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Column names are case sensitive")]
        public void ColumnNamesAreNotCaseSensitive()
        {
            var t = new SimpleTableData();

            t.AddColumn("foo");
            t.AddColumn("FOO");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Duplicates should not be allowed")]
        public void DoesntAllowDuplicates()
        {
            var t = new SimpleTableData();

            t.AddColumn("foo");
            t.AddColumn("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Duplicates should not be allowed")]
        public void PreventsAdditionOfRowsWhenNoColumns()
        {
            var t = new SimpleTableData();
            t.AddRow(new SimpleTableRow());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Row column count not being checked")]
        public void PreventsAdditionOfRowsWithTooManyColumns()
        {
            var t = new SimpleTableData();
            t.AddColumn("col1");
            t.AddColumn("col2");
            t.AddColumn("col3");

            var row = new SimpleTableRow();
            row.Add("val1");
            row.Add("val2");
            row.Add("val3");
            row.Add("val4");

            t.AddRow(row);
        }

        [TestMethod]
        public void AcceptsRowsWithTooFewColumns()
        {
            var t = new SimpleTableData();
            t.AddColumn("col1");
            t.AddColumn("col2");
            t.AddColumn("col3");

            var row = new SimpleTableRow();
            row.Add("val1");

            t.AddRow(row);

            Assert.IsTrue(row.ColumnCount == t.ColumnCount);
        }
    }
}
