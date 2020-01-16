namespace Celcat.Verto.Common
{
    using System;
    using System.Data.SqlClient;

    public sealed class TransactionContext : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private TransactionStatus _status;

        public TransactionContext(string connectionString)
        {
            _connectionString = connectionString;
            _status = TransactionStatus.NotStarted;
        }

        public TransactionStatus Status => _status;

        public void Begin()
        {
            if (_connection != null)
            {
                throw new ApplicationException("Connection already open");
            }

            _connection = new SqlConnection(_connectionString);
            _connection.Open();

            _transaction = _connection.BeginTransaction();
            _status = TransactionStatus.Started;
        }

        public void Commit()
        {
            CheckTransaction();
            _transaction.Commit();
            DisposeTransaction();
        }

        private void DisposeTransaction()
        {
            _status = TransactionStatus.Ended;
            _transaction.Dispose();
            _transaction = null;
        }

        public void Rollback()
        {
            CheckTransaction();
            _transaction.Rollback();
            DisposeTransaction();
        }

        private void CheckTransaction()
        {
            if (_transaction == null)
            {
                throw new ApplicationException("Transaction is null");
            }
        }

        public SqlCommand CreateCommand()
        {
            CheckTransaction();
            var cmd = _connection.CreateCommand();
            cmd.Transaction = _transaction;
            return cmd;
        }

        public SqlTransaction Transaction => _transaction;

        public SqlConnection Connection => _connection;

        public void Dispose()
        {
            if (_transaction != null)
            {
                Rollback();
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
