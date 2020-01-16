namespace UnitTests
{
    using System.Linq;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.TableBuilder.Columns;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestHistoryTableCreator
    {
        [TestMethod]
        public void CanGenerateHistoryTables()
        {
            var b = HistoryTablesBuilder.Get();
            var tables = b.GetTableNames();
            Assert.IsTrue(tables.Any());
        }

        [TestMethod]
        public void HistoryColumnsAreAllNullable()
        {
            // in the history tables we leave all cols as nullable
            var b = HistoryTablesBuilder.Get();
            var tables = b.GetTables();
            foreach (var t in tables)
            {
                var cols = t.Columns;
                foreach (var col in cols)
                {
                    if (!col.Name.Equals("history_federated") && !col.Name.Equals("history_applied"))
                    {
                        Assert.IsTrue(col.Nullable == ColumnNullable.True, "Table = {0}, Col = {1}", t.Name, col.Name);
                    }
                    else
                    {
                        Assert.IsFalse(col.Nullable == ColumnNullable.True, "Table = {0}, Col = {1}", t.Name, col.Name);
                    }
                }
            }
        }
    }
}
