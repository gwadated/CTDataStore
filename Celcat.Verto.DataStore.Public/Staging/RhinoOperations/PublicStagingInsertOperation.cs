namespace Celcat.Verto.DataStore.Public.Staging.RhinoOperations
{
    using Celcat.Verto.Common;
    using Celcat.Verto.TableBuilder;
    using Rhino.Etl.Core.Operations;

    internal class PublicStagingInsertOperation : SqlBulkInsertOperation
    {
        private readonly Table _targetTable;

        public PublicStagingInsertOperation(string connectionString, Table targetTable, int timeoutSecs)
           : base(
               DatabaseUtils.CreateConnectionStringSettings(connectionString),
               DatabaseUtils.GetQualifiedTableName(PublicStagingSchema.StagingSchemaName, targetTable.Name), 
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
        }
    }
}
