namespace Celcat.Verto.TableBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Celcat.Verto.Common;
    using Celcat.Verto.TableBuilder.Columns;
    using global::Common.Logging;
    
    public class Builder
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<IDatabaseObject> _objects;
        
        public Builder()
        {
            _objects = new List<IDatabaseObject>();
        }

        public event EventHandler<ExecuteEventArgs> BeforeExecute;

        public IReadOnlyList<string> GetTableNames()
        {
            return _objects.OfType<Table>().Select(o => o.Name).ToList();
        }

        public bool HasTables => GetTables().Any();

        public IReadOnlyList<Table> GetTables()
        {
            return _objects.OfType<Table>().Select(o => o).ToList();
        }

        public Table GetTable(string tableName)
        {
            foreach (var o in _objects)
            {
                if (o is Table table && table.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return table;
                }
            }

            return null;
        }

        public IReadOnlyList<TableColumn> GetColumns(string tableName)
        {
            return (from o in _objects
                    select o as Table into table
                    where table != null && table.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                    select table.Columns).FirstOrDefault();
        }

        public IReadOnlyList<string> GetColumnNames(string tableName)
        {
            var cols = GetColumns(tableName);
            return cols.Select(c => c.Name).ToList();
        }

        public Table AddTable(Table table)
        {
            Add(table);
            return table;
        }

        private void Add(IDatabaseObject dbObject)
        {
            _objects.Add(dbObject);
        }

        private string GenerateCommand()
        {
            var sb = new StringBuilder();

            foreach (var o in _objects)
            {
                sb.AppendLine(o.GenerateSqlToCreate());
            }

            return sb.ToString();
        }

        public void Execute(TransactionContext tc, int commandTimeoutSecs)
        {
            if (tc.Status == TransactionStatus.NotStarted)
            {
                tc.Begin();
            }

            Execute(tc.Connection, tc.Transaction, commandTimeoutSecs);
        }

        public void Execute(string connectionString, int commandTimeoutSecs)
        {
            string sql = GenerateCommand();
            if (!string.IsNullOrEmpty(sql))
            {
                using (SqlConnection c = DatabaseUtils.CreateConnection(connectionString))
                {
                    Execute(c, null, commandTimeoutSecs);
                }
            }
        }

        public void Execute(SqlConnection c, SqlTransaction tran, int commandTimeoutSecs)
        {
            _log.DebugFormat("Executing Table Builder");

            var sql = GenerateCommand();
            if (!string.IsNullOrEmpty(sql))
            {
                using (SqlCommand cmd = c.CreateCommand())
                {
                    if (tran != null)
                    {
                        cmd.Transaction = tran;
                    }

                    // give caller chance to view and modify sql...
                    var args = new ExecuteEventArgs { SqlStatements = sql };

                    OnBeforeExecute(args);
                    cmd.CommandTimeout = commandTimeoutSecs;
                    cmd.CommandText = args.SqlStatements;
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                _log.Warn("Table Builder has no tables!");
            }
        }

        private void OnBeforeExecute(ExecuteEventArgs e)
        {
            BeforeExecute?.Invoke(this, e);
        }
    }
}
