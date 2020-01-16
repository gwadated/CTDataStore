namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    [XmlRoot("dataStoreConfiguration", IsNullable = false)]
    public class DataStoreConfiguration
    {
        /// <summary>
        /// Connection strings, etc for source data
        /// </summary>
        [XmlElement("source", IsNullable = false)]
        public Source Source { get; set; }

        /// <summary>
        /// Connection strings, etc for destination data
        /// </summary>
        [XmlElement("destination", IsNullable = false)]
        public Destination Destination { get; set; }

        /// <summary>
        /// The command timeouts for various databases are specified here. Note
        /// that these are not the same as the connection timeouts which can be
        /// specified in the connection strings themselves
        /// </summary>
        [XmlElement("commandTimeouts")]
        public CommandTimeout CommandTimeouts { get; set; }

        /// <summary>
        /// SQL Bulk insert is used to improve performance during transformation of 
        /// public staging rows. You can disable it here, e.g. for debug/test purposes
        /// </summary>
        [XmlElement("disableBulkInsertOptimisations")]
        public bool DisableBulkInsertOptimisations { get; set; }

        /// <summary>
        /// Force a complete rebuild of the ADMIN and PUBLIC databases
        /// </summary>
        [XmlElement("forceRebuild")]
        public bool ForceRebuild { get; set; }

        /// <summary>
        /// Max degree of parallelism (-1 for no limit)
        /// </summary>
        [XmlElement("maxDegreeOfParallelism")]
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// Truncate the public staging tables
        /// </summary>
        [XmlElement("truncatePublicStaging")]
        public bool TruncatePublicStaging { get; set; }

        /// <summary>
        /// Pipeline attributes
        /// </summary>
        [XmlElement("pipelines")]
        public Pipelines Pipelines { get; set; }

        [XmlElement("consolidation")]
        public ConsolidationParams Consolidation { get; set; }

        public DataStoreConfiguration()
        {
            Source = new Source();
            Destination = new Destination();
            CommandTimeouts = new CommandTimeout();
            Consolidation = new ConsolidationParams();

            // default values...
            MaxDegreeOfParallelism = -1;
            TruncatePublicStaging = true;
        }

        public static DataStoreConfiguration Load(string xmlConfigFile)
        {
            ConfigurationFile cf = new ConfigurationFile();
            return cf.Read(xmlConfigFile);
        }

        public static void Save(string xmlConfigFile, DataStoreConfiguration c)
        {
            ConfigurationFile cf = new ConfigurationFile();
            cf.Write(xmlConfigFile, c);
        }

        public static DataStoreConfiguration Load(Stream stream)
        {
            var cf = new ConfigurationFile();
            return cf.Read(stream);
        }

        public static void Save(StreamWriter stream, DataStoreConfiguration c)
        {
            var cf = new ConfigurationFile();
            cf.Write(stream, c);
        }

        public IReadOnlyList<string> TimetableConnectionStrings
        {
            get => Source.Timetables.Select(tt => tt.ConnectionString).ToList();
        }

        public void CheckValidity()
        {
            CheckAdminDatabaseConnectionStringValidity();

            if (TimetableConnectionStrings == null || !TimetableConnectionStrings.Any())
            {
                throw new ApplicationException("No source timetables are specified");
            }

            var serverAndDbNames = new List<string>();

            foreach (var t in TimetableConnectionStrings)
            {
                CheckConnectionStringValidity(serverAndDbNames, t);
            }

            if (MaxDegreeOfParallelism < -1 || MaxDegreeOfParallelism == 0)
            {
                throw new ApplicationException("MaxDegreeOfParallelism must be -1 (for no limit) or > 0");
            }
        }

        private void CheckAdminDatabaseConnectionStringValidity()
        {
            if (string.IsNullOrWhiteSpace(Destination.AdminDatabase.ConnectionString))
            {
                throw new ApplicationException("Admin database connection string is not specified");
            }

            // ReSharper disable once CollectionNeverQueried.Local
            var sb = new SqlConnectionStringBuilder(Destination.AdminDatabase.ConnectionString);

            if (string.IsNullOrWhiteSpace(sb.DataSource))
            {
                throw new ApplicationException("Admin database server name is not specified");
            }

            if (string.IsNullOrWhiteSpace(sb.InitialCatalog))
            {
                throw new ApplicationException("Admin database name is not specified");
            }
        }

        private void CheckConnectionStringValidity(List<string> serverAndDbNames, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("Source timetable connection string is empty");
            }

            // ReSharper disable once CollectionNeverQueried.Local
            var sb = new SqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrWhiteSpace(sb.DataSource))
            {
                throw new ApplicationException(
                    $"Server name is not specified for source timetable: {DatabaseUtils.GetConnectionDescription(connectionString)}");
            }

            if (string.IsNullOrWhiteSpace(sb.InitialCatalog))
            {
                throw new ApplicationException(
                    $"Database name is not specified for source timetable: {DatabaseUtils.GetConnectionDescription(connectionString)}");
            }

            var serverAndDb = string.Concat(sb.DataSource, ":", sb.InitialCatalog);
            if (serverAndDbNames.Contains(serverAndDb, StringComparer.OrdinalIgnoreCase))
            {
                throw new ApplicationException(
                    $"Duplicate connection found for source timetable: {DatabaseUtils.GetConnectionDescription(connectionString)}");
            }

            serverAndDbNames.Add(serverAndDb);
        }

        public void ClearConsolidationColumns()
        {
            if (!Consolidation.Enabled)
            {
                Consolidation.ClearEntries();
            }
        }
    }
}
