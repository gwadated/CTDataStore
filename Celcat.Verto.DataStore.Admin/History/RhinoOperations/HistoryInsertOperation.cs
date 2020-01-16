namespace Celcat.Verto.DataStore.Admin.History.RhinoOperations
{
    using System;
    using Celcat.Verto.Common;
    using Celcat.Verto.TableBuilder;
    using Rhino.Etl.Core.Operations;

    internal class HistoryInsertOperation : SqlBulkInsertOperation
    {
        private readonly Table _targetTable;

        public HistoryInsertOperation(string connectionString, Table targetTable, int timeoutSecs)
           : base(
               DatabaseUtils.CreateConnectionStringSettings(connectionString),
               DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, targetTable.Name), 
               timeoutSecs)
        {
            _targetTable = targetTable;
        }

        protected override void PrepareSchema()
        {
            foreach (var c in _targetTable.Columns)
            {
                Schema[c.Name] = DbTypeMatching.GetClrType(c.SqlDbType);
            }

            Schema[HistorySchema.HistoryStatusColumnName] = typeof(string);
            Schema[HistorySchema.HistoryStampColumnName] = typeof(DateTime);
            Schema[HistorySchema.HistoryLogColumnName] = typeof(long);
            Schema[HistorySchema.HistoryFederatedColumnName] = typeof(bool);
            Schema[HistorySchema.HistoryAppliedColumnName] = typeof(bool);
        }
    }
}
