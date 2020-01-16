namespace UnitTests
{
    using System.Linq;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.TableBuilder.Columns;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestStagingTableCreator
    {
        [TestMethod]
        public void CanGenerateStagingTables()
        {
            var b = StagingTablesBuilder.Get("dummy");
            var tables = b.GetTableNames();
            Assert.IsTrue(tables.Any());
        }

        [TestMethod]
        public void StagingColumnsAreAllNullable()
        {
            // in the staging tables we leave all cols as nullable

            var b = StagingTablesBuilder.Get("dummy");
            var tables = b.GetTables();
            foreach (var t in tables)
            {
                var cols = t.Columns;
                foreach (var col in cols)
                {
                    Assert.IsTrue(col.Nullable == ColumnNullable.True, "Table = {0}, Col = {1}", t.Name, col.Name);
                }
            }
        }
    }
}
