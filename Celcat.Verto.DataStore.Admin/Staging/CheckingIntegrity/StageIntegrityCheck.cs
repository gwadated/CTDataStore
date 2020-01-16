namespace Celcat.Verto.DataStore.Admin.Staging.CheckingIntegrity
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;

    internal class StageIntegrityCheck
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private readonly string _schemaName;
        private readonly int _timeoutSecs;
        private readonly int _maxDegreeOfParallelism;
        
        public StageIntegrityCheck(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, string schemaName)
        {
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _schemaName = schemaName;
        }

        /// <summary>
        /// Checks integrity of stage data. Because we extract a table at a time from the source data
        /// it's possible that we may find broken references. This situation is unnaceptable so this check
        /// will throw an exception allowing us to quit the process whilst still in temp staging.
        /// </summary>
        public void Execute()
        {
            var b = StagingTablesBuilder.Get(_schemaName);
            DoParallelProcessing(b);
        }

        private void DoParallelProcessing(StagingTablesBuilder b)
        {
            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism };

            Parallel.ForEach(b.GetTables(), pOptions, (table, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    int issuesFound = Check(table);
                    if (issuesFound > 0)
                    {
                        loopState.Stop();
                        throw new ApplicationException($"Found {issuesFound} faulty rows in {table.Name}");
                    }
                }
            });
        }

        private int Check(Table table)
        {
            string qualifiedTableName = DatabaseUtils.GetQualifiedTableName(_schemaName, table.Name);

            _log.DebugFormat("Checking integrity of table: {0}", qualifiedTableName);

            int totalIssues = 0;

            var colrefChecks = table.ColumnReferenceChecks;
            foreach (var chk in colrefChecks)
            {
                var issuesFound = chk is ResourceIdReferenceCheck chkRes
                   ? GenericResourceColumnCheck(chkRes, qualifiedTableName)
                   : ResourceColumnCheck(chk, qualifiedTableName);

                if (issuesFound > 0)
                {
                    _log.WarnFormat(
                        "Checked staging table: {0} - {1} broken references ({2})", qualifiedTableName, issuesFound, chk.LocalColumnNamesAsCsv());
                }

                totalIssues += issuesFound;
            }

            return totalIssues;
        }

        private int ResourceColumnCheck(ColumnReferenceCheck chk, string qualifiedTableName)
        {
            // e.g.
            // select count(1) from STAGETMP.CT_COURSE a
            // where dept_id is not NULL and
            // not exists(select 1 from STAGETMP.CT_DEPT b where a.dept_id = b.dept_id)

            var localCols = chk.LocalColumnNames;
            var primaryCols = chk.PrimaryColumnNames;

            var sb = new SqlBuilder();
            sb.Append("select count(1) from");
            sb.Append(qualifiedTableName);
            sb.Append("a where");

            for (int n = 0; n < chk.ColumnCount; ++n)
            {
                if (n > 0)
                {
                    sb.Append("and");
                }

                sb.AppendFormat("{0} is not NULL", localCols[n]);
            }

            sb.AppendFormat("and not exists(select 1 from {0} b", DatabaseUtils.GetQualifiedTableName(_schemaName, chk.PrimaryTableName));
            sb.Append("where");

            for (int n = 0; n < chk.ColumnCount; ++n)
            {
                if (n > 0)
                {
                    sb.Append("and");
                }

                sb.AppendFormat("a.{0}=b.{1}", localCols[n], primaryCols[n]);
            }

            sb.Append(")");

            return Convert.ToInt32(DatabaseUtils.ExecuteScalar(_connectionString, sb.ToString(), _timeoutSecs));
        }

        private int GenericResourceColumnCheck(ResourceIdReferenceCheck chk, string qualifiedTableName)
        {
            // special case. We are dealing with a "resource_id" column (always associated with a 
            // corresponding "resource_type" column) which is used in Ct7 as an "untyped" id. We
            // know which entity it points to by reference to the resource_type value.
            int issuesFound = 0;

            var localCols = chk.LocalColumnNames;

            if (localCols.Count != 2 || !localCols[0].Contains("resource_type") || !localCols[1].Contains("resource_id"))
            {
                throw new ApplicationException("Unexpected columns");
            }

            var resTypeColName = localCols[0];
            var resIdColName = localCols[1];

            // e.g.
            // select count(1) from STAGETMP.CT_GROUP_FP
            // where resource_type = 601 and resource_id is not NULL
            // and not exists(select 1 from STAGETMP.CT_MODULE where module_id = STAGETMP.CT_GROUP_FP.resource_id)

            // get all of the resource_type values in this table...
            var resourceTypes = GetResourceTypesInTable(resTypeColName, qualifiedTableName);
            foreach (var i in resourceTypes)
            {
                var resType = (Ct7Entity)i;
                var qualifiedPrimaryTableName =
                   DatabaseUtils.GetQualifiedTableName(_schemaName, Ct7EntityUtils.GetTableName(resType));

                var primaryIdColName = Ct7EntityUtils.GetIdFldName(resType);

                var sb = new SqlBuilder();
                sb.Append("select count(1) from");
                sb.Append(qualifiedTableName);
                sb.AppendFormat("where {0}={1}", resTypeColName, (int)resType);
                sb.AppendFormat("and {0} is not NULL", resIdColName);
                sb.AppendFormat("and not exists(select 1 from {0}", qualifiedPrimaryTableName);
                sb.AppendFormat("where {0} = {1}.{2})", primaryIdColName, qualifiedTableName, resIdColName);

                issuesFound = Convert.ToInt32(DatabaseUtils.ExecuteScalar(_connectionString, sb.ToString(), _timeoutSecs));
            }

            return issuesFound;
        }

        private IReadOnlyCollection<int> GetResourceTypesInTable(string resTypeColName, string qualifiedTableName)
        {
            var results = new List<int>();

            string sql = $"select distinct {resTypeColName} from {qualifiedTableName}";

            DatabaseUtils.EnumerateResults(_connectionString, sql, _timeoutSecs, r =>
            {
                var o = r[resTypeColName];
                if (o != DBNull.Value)
                {
                    results.Add((int)o);
                }
            });

            return results;
        }
    }
}
