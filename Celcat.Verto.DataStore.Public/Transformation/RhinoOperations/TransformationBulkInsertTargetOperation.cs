namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.TableBuilder;
    using Rhino.Etl.Core.Operations;

    internal class TransformationBulkInsertTargetOperation : SqlBulkInsertOperation
    {
        private readonly Table _targetTable;

        public TransformationBulkInsertTargetOperation(string connectionString, int timeoutSecs, PublicTable targetTable)
           : base(DatabaseUtils.CreateConnectionStringSettings(connectionString), targetTable.QualifiedName, timeoutSecs)
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
