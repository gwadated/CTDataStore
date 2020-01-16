namespace Celcat.Verto.DataStore.Admin.SourceTimetables
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Models;
    using Celcat.Verto.DataStore.Admin.Staging;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Entities;
    using global::Common.Logging;

    internal static class SourceValidityCheck
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Checks that all source databases exist, can be accessed, and are likely to be timetable databases!
        /// </summary>
        public static IReadOnlyList<SourceTimetableData> Execute(
            IReadOnlyList<string> sourceConnections, int commandTimeoutSecs, int maxDegreeOfParallelism)
        {
            _log.DebugFormat("Checking {0} source databases", sourceConnections.Count);

            List<SourceTimetableData> result;
            try
            {
                result = DoParallelProcessing(sourceConnections, commandTimeoutSecs, maxDegreeOfParallelism);
                CheckNoDuplicates(result);
                CheckIdentityColumns(result, commandTimeoutSecs);
            }
            catch (AggregateException ex)
            {
                // just throw the first exception...
                throw ex.InnerExceptions[0];
            }

            return result.AsReadOnly();
        }

        private static List<SourceTimetableData> DoParallelProcessing(
            IReadOnlyList<string> sourceConnections, int commandTimeoutSecs, int maxDegreeOfParallelism)
        {
            var result = new List<SourceTimetableData>();

            var locker = new object();

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

            Parallel.ForEach(sourceConnections, pOptions, (src, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    var ttDescription = DatabaseUtils.GetConnectionDescription(src);

                    _log.DebugFormat("Checking source database: {0}", ttDescription);

                    var srcTimetable = new SourceTimetable(src, commandTimeoutSecs);

                    if (!srcTimetable.DatabaseExists())
                    {
                        loopState.Stop();

                        string errMsg = $"Source timetable database doesn't exist: {ttDescription}";
                        _log.Error(errMsg);
                        throw new ApplicationException(errMsg);
                    }

                    int ver = srcTimetable.GetSchemaVersion();
                    if (ver == 0)
                    {
                        loopState.Stop();

                        string errMsg = $"Source timetable database version not found: {ttDescription}";
                        _log.Error(errMsg);
                        throw new ApplicationException(errMsg);
                    }

                    // compare the latest staging schema with the rows in the source timetable...
                    var report = srcTimetable.GetCompatibilityWithStagingSchema();
                    if (!report.IsCompatible)
                    {
                        loopState.Stop();

                        string errMsg = $"Incompatible source timetable: {ttDescription} - {report}";
                        _log.Error(errMsg);
                        throw new ApplicationException(errMsg);
                    }

                    var ttData = GetSourceTimetableData(src, commandTimeoutSecs);
                    lock (locker)
                    {
                        result.Add(ttData);
                    }

                    _log.DebugFormat("Source timetable is compatible: {0}", ttDescription);
                }
            });

            return result;
        }

        private static void CheckSamePrimaryKeyColumns(List<Dictionary<string, PrimaryKeyInfo>> allPkInfo, StagingTablesBuilder b)
        {
            // checks that all source timetables have identical primary key col names for 
            // the tables that are to be extracted, which is an important consideration when 
            // performing the full database diff...
            foreach (var tableName in b.GetTableNames(includePseudoRegisterMarkTable: false))
            {
                PrimaryKeyInfo baseInfo = null;

                foreach (var info in allPkInfo)
                {
                    var pkInfo = info[tableName];
                    if (pkInfo == null)
                    {
                        throw new ApplicationException($"Could not find primary key data for table: {tableName}");
                    }

                    if (baseInfo == null)
                    {
                        baseInfo = pkInfo;
                    }
                    else
                    {
                        if (!PrimaryKeyInfo.Identical(baseInfo, pkInfo))
                        {
                            var sb = new StringBuilder();
                            sb.Append("There is an incompatibility between source timetables.");
                            sb.AppendFormat(" Primary key column definitions are different for table {0}", tableName);

                            throw new ApplicationException(sb.ToString());
                        }
                    }
                }
            }
        }

        private static void CheckIdentityColumns(IReadOnlyList<SourceTimetableData> timetables, int commandTimeoutSecs)
        {
            _log.Debug("Checking primary keys");

            var b = StagingTablesBuilder.Get(StagingSchema.PrimaryStagingSchemaName);

            var pkInfoForAllTimetables = new List<Dictionary<string, PrimaryKeyInfo>>();

            foreach (var tt in timetables)
            {
                var pkInfo = DatabaseUtils.GetPrimaryKeyInfo(tt.ConnectionString, commandTimeoutSecs);
                pkInfoForAllTimetables.Add(pkInfo);
            }

            if (pkInfoForAllTimetables.Any())
            {
                CheckSamePrimaryKeyColumns(pkInfoForAllTimetables, b);
                CheckStagingTablesListPrimaryKeysFirst(pkInfoForAllTimetables[0]);
                CheckEntitiesHaveSinglePrimaryKeyId(pkInfoForAllTimetables[0]);
            }
        }

        private static void CheckEntitiesHaveSinglePrimaryKeyId(Dictionary<string, PrimaryKeyInfo> info)
        {
            // checks all of our Entity types have a single primary key id
            // since these are the entities that will receive a new master Id
            // for use in the public store...
            foreach (Entity e in Enum.GetValues(typeof(Entity)))
            {
                if (e != Entity.Unknown)
                {
                    var i = info[EntityUtils.ToCtTableName(e)];
                    if (i == null)
                    {
                        throw new ApplicationException($"Could not find primary key data for entity: {e}");
                    }

                    if (i.Columns.Count != 1)
                    {
                        throw new ApplicationException($"Entity has more than one primary key column: {e}");
                    }
                }
            }
        }

        private static void CheckStagingTablesListPrimaryKeysFirst(Dictionary<string, PrimaryKeyInfo> pkInfoList)
        {
            // it's important that all staging tables list the primary keys first because this is assumed
            // by the algorithm that performs table diffs.
            var b = StagingTablesBuilder.Get(StagingSchema.PrimaryStagingSchemaName);
            foreach (var table in b.GetTables(includePseudoRegisterMarkTable: false))
            {
                if (!table.Name.Equals("CT_CONFIG", StringComparison.OrdinalIgnoreCase))
                {
                    PrimaryKeyInfo pkInfo = pkInfoList[table.Name];
                    if (pkInfo != null)
                    {
                        var stagingTableCols = table.Columns;

                        // deduct 1 for src_timetable_id
                        if (stagingTableCols.Count - 1 < pkInfo.Columns.Count)
                        {
                            throw new ApplicationException(
                                $"Staging table contains fewer columns than primary key of source: {table.Name}");
                        }

                        for (int n = 0; n < pkInfo.Columns.Count; ++n)
                        {
                            if (!pkInfo.Columns[n].ColumnName.Equals(stagingTableCols[n + 1].Name, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new ApplicationException(
                                    $"Incorrect staging table column order: {table.Name} (primary key columns must be defined first)");
                            }
                        }
                    }
                }
            }
        }

        private static void CheckNoDuplicates(IEnumerable<SourceTimetableData> result)
        {
            var ids = new Dictionary<Guid, string>();

            foreach (var tt in result)
            {
                if (ids.ContainsKey(tt.Identifier))
                {
                    throw new ApplicationException(
                        $"Duplicate timetables found: [{tt.Name}] and [{ids[tt.Identifier]}]. Both have the same guid");
                }

                ids[tt.Identifier] = tt.Name;
            }
        }

        private static SourceTimetableData GetSourceTimetableData(string timetableConnectionString, int timeoutSecs)
        {
            SourceTimetableData result = null;
            
            var csb = new SqlConnectionStringBuilder(timetableConnectionString);

            var sql = "select timetable_name, version, guid from CT_CONFIG";
            DatabaseUtils.GetSingleResult(timetableConnectionString, sql, timeoutSecs, r =>
            {
                result = new SourceTimetableData
                {
                    Name = (string)r["timetable_name"],
                    SqlServerName = csb.DataSource,
                    DatabaseName = csb.InitialCatalog,
                    Identifier = (Guid)r["guid"],
                    SchemaVersion = (int)r["version"],
                    ConnectionString = timetableConnectionString
                };
            });

            return result;
        }
    }
}
