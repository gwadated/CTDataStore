namespace Celcat.Verto.DataStore.Common.RhinoDerivatives
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Celcat.Verto.Common;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.DataReaders;
    using Rhino.Etl.Core.Operations;

    // adapted from Rhino ETl SqlBulkInsertOperation class 
    public abstract class SqlBulkInsertOperationUsingExistingConnection : AbstractDatabaseOperation
    {
        /// <summary>
        /// The mapping of columns from the row to the database schema.
        /// Important: The column name in the database is case sensitive!
        /// </summary>
        private readonly IDictionary<string, string> _mappings = new Dictionary<string, string>();
        private readonly IDictionary<string, Type> _inputSchema = new Dictionary<string, Type>();
        private readonly string _targetTable;
        private readonly int _timeout;
        private readonly SqlConnection _sqlConnection;

        private IDictionary<string, Type> _schema = new Dictionary<string, Type>();
        private int _batchSize;
        private int _notifyBatchSize;
        private SqlBulkCopyOptions _bulkCopyOptions = SqlBulkCopyOptions.Default;
        
        protected SqlBulkInsertOperationUsingExistingConnection(SqlConnection c, string targetTable, int timeoutSecs)
            : base(DatabaseUtils.CreateConnectionStringSettings(c.ConnectionString))
        {
            _targetTable = targetTable;
            _timeout = timeoutSecs;
            _sqlConnection = c;
        }

        /// <summary>The batch size value of the bulk insert operation</summary>
        public int BatchSize
        {
            get => _batchSize;
            set => _batchSize = value;
        }

        ///    <summary>The batch size    value of the bulk insert operation</summary>
        public int NotifyBatchSize
        {
            get => _notifyBatchSize > 0 ? _notifyBatchSize : _batchSize;
            set => _notifyBatchSize = value;
        }

        /// <summary><c>true</c> to turn the <see cref="SqlBulkCopyOptions.TableLock"/> option on, otherwise <c>false</c>.</summary>
        public bool LockTable
        {
            get => IsOptionOn(SqlBulkCopyOptions.TableLock);
            set => ToggleOption(SqlBulkCopyOptions.TableLock, value);
        }

        /// <summary><c>true</c> to turn the <see cref="SqlBulkCopyOptions.KeepIdentity"/> option on, otherwise <c>false</c>.</summary>
        public bool KeepIdentity
        {
            get => IsOptionOn(SqlBulkCopyOptions.KeepIdentity);
            set => ToggleOption(SqlBulkCopyOptions.KeepIdentity, value);
        }

        /// <summary><c>true</c> to turn the <see cref="SqlBulkCopyOptions.KeepNulls"/> option on, otherwise <c>false</c>.</summary>
        public bool KeepNulls
        {
            get => IsOptionOn(SqlBulkCopyOptions.KeepNulls);
            set => ToggleOption(SqlBulkCopyOptions.KeepNulls, value);
        }

        /// <summary><c>true</c> to turn the <see cref="SqlBulkCopyOptions.CheckConstraints"/> option on, otherwise <c>false</c>.</summary>
        public bool CheckConstraints
        {
            get => IsOptionOn(SqlBulkCopyOptions.CheckConstraints);
            set => ToggleOption(SqlBulkCopyOptions.CheckConstraints, value);
        }

        /// <summary><c>true</c> to turn the <see cref="SqlBulkCopyOptions.FireTriggers"/> option on, otherwise <c>false</c>.</summary>
        public bool FireTriggers
        {
            get => IsOptionOn(SqlBulkCopyOptions.FireTriggers);
            set => ToggleOption(SqlBulkCopyOptions.FireTriggers, value);
        }

        private void ToggleOption(SqlBulkCopyOptions option, bool on)
        {
            if (on)
            {
                TurnOptionOn(option);
            }
            else
            {
                TurnOptionOff(option);
            }
        }

        /// <summary>Returns <c>true</c> if the <paramref name="option"/> is turned on, otherwise <c>false</c></summary>
        /// <param name="option">The <see cref="SqlBulkCopyOptions"/> option to test for.</param>
        private bool IsOptionOn(SqlBulkCopyOptions option)
        {
            return (_bulkCopyOptions & option) == option;
        }

        /// <summary>Turns the <paramref name="option"/> on.</summary>
        /// <param name="option"></param>
        private void TurnOptionOn(SqlBulkCopyOptions option)
        {
            _bulkCopyOptions |= option;
        }

        /// <summary>Turns the <paramref name="option"/> off.</summary>
        /// <param name="option"></param>
        private void TurnOptionOff(SqlBulkCopyOptions option)
        {
            if (IsOptionOn(option))
            {
                _bulkCopyOptions ^= option;
            }
        }

        /// <summary>The table or view's schema information.</summary>
        public IDictionary<string, Type> Schema
        {
            get => _schema;
            set => _schema = value;
        }

        /// <summary>
        /// Prepares the mapping for use, by default, it uses the schema mapping.
        /// This is the preferred approach
        /// </summary>
        public void PrepareMapping()
        {
            foreach (KeyValuePair<string, Type> pair in _schema)
            {
                _mappings[pair.Key] = pair.Key;
            }
        }

        /// <summary>Use the destination Schema and Mappings to create the
        /// operations input schema so it can build the adapter for sending
        /// to the WriteToServer method.</summary>
        public void CreateInputSchema()
        {
            foreach (KeyValuePair<string, string> pair in _mappings)
            {
                _inputSchema.Add(pair.Key, _schema[pair.Value]);
            }
        }

        /// <summary>
        /// Executes this operation
        /// </summary>
        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            Guard.Against<ArgumentException>(rows == null, "SqlBulkInsertOperation cannot accept a null enumerator");
            PrepareSchema();
            PrepareMapping();
            CreateInputSchema();

            var sqlBulkCopy = CreateSqlBulkCopy();
            var adapter = new DictionaryEnumeratorDataReader(_inputSchema, rows);
            sqlBulkCopy.WriteToServer(adapter);

            yield break;
        }

        /// <summary>
        ///    Handle sql notifications
        ///    </summary>
        private void OnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            Debug("{0} rows    copied to database", e.RowsCopied);
        }

        ///    <summary>
        /// Prepares the schema of the target table
        /// </summary>
        protected abstract void PrepareSchema();

        /// <summary>
        /// Creates the SQL bulk copy instance
        /// </summary>
        private SqlBulkCopy CreateSqlBulkCopy()
        {
            var copy = new SqlBulkCopy(_sqlConnection, _bulkCopyOptions, null) { BatchSize = _batchSize };
            foreach (KeyValuePair<string, string> pair in _mappings)
            {
                copy.ColumnMappings.Add(pair.Key, pair.Value);
            }

            copy.NotifyAfter = NotifyBatchSize;
            copy.SqlRowsCopied += OnSqlRowsCopied;
            copy.DestinationTableName = _targetTable;
            copy.BulkCopyTimeout = _timeout;
            return copy;
        }
    }
}
