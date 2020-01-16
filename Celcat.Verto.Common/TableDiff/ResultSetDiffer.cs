namespace Celcat.Verto.Common.TableDiff
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Reflection;
    using global::Common.Logging;

    public class ResultSetDiffer : DifferBase
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private readonly string _sqlSelectOldContent;
        private readonly string _sqlSelectNewContent;
        private readonly int _commandTimeoutSecs;
        
        protected ResultSetDiffer(
            string connectionString, 
            int commandTimeoutSecs,
            string sqlSelectOldContent, 
            string sqlSelectNewContent, 
            int primaryKeyColCount = 1)
           : base(primaryKeyColCount)
        {
            _connectionString = connectionString;
            _commandTimeoutSecs = commandTimeoutSecs;
            _sqlSelectOldContent = sqlSelectOldContent;
            _sqlSelectNewContent = sqlSelectNewContent;
        }

        protected override SimpleTableData GetRowsWithDifferences()
        {
            var result = new SimpleTableData();

            using (var c = CreateConnection())
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandTimeout = _commandTimeoutSecs;
                cmd.CommandText = GenerateSql();
                using (IDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        result.AddFromReader(r);
                    }
                }
            }

            return result;
        }

        private SqlConnection CreateConnection()
        {
            return DatabaseUtils.CreateConnection(_connectionString);
        }

        private string GenerateSql()
        {
            var sb = new SqlBuilder();

            _log.DebugFormat("Comparing results sets: {0} and {1}", _sqlSelectOldContent, _sqlSelectNewContent);

            sb.AppendFormat("select {0} = 1, * from", TableNumberColName);
            sb.Append("(");
            sb.Append(_sqlSelectOldContent);
            sb.Append("except");
            sb.Append(_sqlSelectNewContent);
            sb.Append(") x");

            sb.Append("union all");

            sb.AppendFormat("select {0} = 2, * from", TableNumberColName);
            sb.Append("(");
            sb.Append(_sqlSelectNewContent);
            sb.Append("except");
            sb.Append(_sqlSelectOldContent);
            sb.Append(") y");

            return sb.ToString();
        }
    }
}
