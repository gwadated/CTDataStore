namespace Celcat.Verto.Common.BatchDeletes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class BatchDelete
    {
        private const int MaxBatchSize = 20;

        private readonly TransactionContext _transactionContext;
        private readonly string _connectionString;
        private int _timeoutSecs;
        private string _tableName;
        private string _idColName;
        private List<long> _ids;
        private int _currentIndex;

        public BatchDelete(string connectionString, int timeoutSecs, string tableName, string idColName)
        {
            _connectionString = connectionString;
            Init(timeoutSecs, tableName, idColName);
        }

        public BatchDelete(TransactionContext tc, int timeoutSecs, string tableName, string idColName)
        {
            _transactionContext = tc;
            Init(timeoutSecs, tableName, idColName);
        }

        public void Execute()
        {
            var ids = GetNextBatch();
            while (ids.Any())
            {
                var b = new SqlBuilder();
                b.AppendFormat("delete from {0}", _tableName);
                b.AppendFormat("where {0} in (", _idColName);

                for (var n = 0; n < ids.Count; ++n)
                {
                    if (n > 0)
                    {
                        b.AppendNoSpace(",");
                    }

                    b.AppendNoSpace(ids[n].ToString());
                }

                b.Append(")");

                if (_transactionContext != null)
                {
                    DatabaseUtils.ExecuteSql(_transactionContext, b.ToString(), _timeoutSecs);
                }
                else
                {
                    DatabaseUtils.ExecuteSql(_connectionString, b.ToString(), _timeoutSecs);
                }

                ids = GetNextBatch();
            }
        }

        public void Add(long id)
        {
            _ids.Add(id);
        }

        private void Init(int timeoutSecs, string tableName, string idColName)
        {
            _timeoutSecs = timeoutSecs;
            _tableName = tableName;
            _idColName = idColName;

            _ids = new List<long>();
            _currentIndex = 0;
        }

        private List<long> GetNextBatch()
        {
            var batch = new List<long>();

            var maxIndex = Math.Min(_currentIndex + MaxBatchSize, _ids.Count);
            while (_currentIndex < maxIndex)
            {
                batch.Add(_ids[_currentIndex++]);
            }

            return batch;
        }
    }
}
