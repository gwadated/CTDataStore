namespace UnitTests
{
    using System.Linq;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestPublicTableCreator
    {
        private void CanGeneratePublicTables<T>() 
            where T : PublicTable
        {
            var b = new PublicTablesBuilder<T>();
            var tables = b.GetTableNames();
            Assert.IsTrue(tables.Any());
        }

        [TestMethod]
        public void CanGenerateResourceTables()
        {
            CanGeneratePublicTables<PublicResourceTable>();
            CanGeneratePublicTables<PublicAttendanceTable>();
            CanGeneratePublicTables<PublicBookingTable>();
            CanGeneratePublicTables<PublicEventTable>();
            CanGeneratePublicTables<PublicExamTable>();
            CanGeneratePublicTables<PublicMembershipTable>();
            CanGeneratePublicTables<PublicMiscTable>();
        }

        private void PublicTablesHavePrimaryKey<T>() 
            where T : PublicTable
        {
            var b = new PublicTablesBuilder<T>();
            var tables = b.GetTables();
            foreach (var t in tables)
            {
                Assert.IsNotNull(t.PrimaryKey);
            }
        }

        [TestMethod]
        public void PublicTablesHavePrimaryKey()
        {
            PublicTablesHavePrimaryKey<PublicResourceTable>();
            PublicTablesHavePrimaryKey<PublicAttendanceTable>();
            PublicTablesHavePrimaryKey<PublicBookingTable>();
            PublicTablesHavePrimaryKey<PublicEventTable>();
            PublicTablesHavePrimaryKey<PublicExamTable>();
            PublicTablesHavePrimaryKey<PublicMembershipTable>();
            PublicTablesHavePrimaryKey<PublicMiscTable>();
        }
        
        private void PublicTablesHaveNoUniqueKeyOnName<T>() 
            where T : PublicTable
        {
            var b = new PublicTablesBuilder<T>();
            var tables = b.GetTables();
            foreach (var t in tables)
            {
                if (t.ColumnExists("unique_name"))
                {
                    foreach (var idx in t.Indexes)
                    {
                        if (idx.Contains("unique_name"))
                        {
                            Assert.IsFalse(idx.IsUnique);
                        }
                    }
                }

                if (t.ColumnExists("name"))
                {
                    foreach (var idx in t.Indexes)
                    {
                        if (idx.Contains("name"))
                        {
                            Assert.IsFalse(idx.IsUnique);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void PublicTablesHaveNoUniqueKeyOnName()
        {
            PublicTablesHaveNoUniqueKeyOnName<PublicResourceTable>();
            PublicTablesHaveNoUniqueKeyOnName<PublicAttendanceTable>();
            PublicTablesHaveNoUniqueKeyOnName<PublicBookingTable>();
            PublicTablesHaveNoUniqueKeyOnName<PublicEventTable>();
            PublicTablesHaveNoUniqueKeyOnName<PublicExamTable>();
            PublicTablesHaveNoUniqueKeyOnName<PublicMembershipTable>();
            PublicTablesHaveNoUniqueKeyOnName<PublicMiscTable>();
        }
    }
}
