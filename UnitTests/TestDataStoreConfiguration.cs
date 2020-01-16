namespace UnitTests
{
    using System.IO;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestDataStoreConfiguration
    {
        [TestMethod]
        public void CanSaveAndLoad()
        {
            var cf = new ConfigurationFile();

            var c = new DataStoreConfiguration();
            c.CommandTimeouts.SourceTimetables = 300;
            c.CommandTimeouts.AdminDatabase = 400;
            c.CommandTimeouts.PublicDatabase = 500;

            // dummy connection strings - not actually used in the test to connect to a server!
            c.Destination.AdminDatabase.ConnectionString = @"Server = MyServer; Database = CTDS_ADMIN; Trusted_Connection = True";
            c.Destination.PublicDatabase.ConnectionString = @"Server = MyServer; Database = CTDS_PUBLIC; Trusted_Connection = True";

            var tt = new SourceTimetableConfiguration();
            tt.ConnectionString = @"Server = MyServer; Database = MyTimetable; Trusted_Connection = True";
            c.Source.Timetables.Add(tt);

            using (Stream stream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(stream))
            {
                cf.Write(writer, c);

                stream.Position = 0;
                DataStoreConfiguration c2 = cf.Read(stream);

                Assert.AreEqual(c.CommandTimeouts.AdminDatabase, c2.CommandTimeouts.AdminDatabase);
                Assert.AreEqual(c.CommandTimeouts.PublicDatabase, c2.CommandTimeouts.PublicDatabase);
                Assert.AreEqual(c.CommandTimeouts.SourceTimetables, c2.CommandTimeouts.SourceTimetables);
                Assert.AreEqual(c.Destination.AdminDatabase.ConnectionString, c2.Destination.AdminDatabase.ConnectionString);
                Assert.AreEqual(c.Destination.PublicDatabase.ConnectionString, c2.Destination.PublicDatabase.ConnectionString);
                Assert.AreEqual(c.Source.Timetables[0].ConnectionString, c2.Source.Timetables[0].ConnectionString);
            }
        }
    }
}
