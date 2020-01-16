namespace Celcat.Verto.DataStore.Public.Schemas.TempUpsert
{
    using System.Data.SqlClient;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Common.RhinoDerivatives;
    using Celcat.Verto.TableBuilder;

    internal class TempUpsertWriteOperation : SqlBulkInsertOperationUsingExistingConnection
    {
        private readonly Table _targetTable;

        public TempUpsertWriteOperation(SqlConnection c, int timeoutSecs, Table targetTable)
           : base(c, targetTable.QualifiedName, timeoutSecs)
        {
            _targetTable = targetTable;
        }

        protected override void PrepareSchema()
        {
            foreach (var c in _targetTable.Columns)
            {
                Schema[c.Name] = DbTypeMatching.GetClrType(c.SqlDbType);
            }
        }
    }
}
