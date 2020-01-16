namespace Celcat.Verto.DataStore.Admin.Staging.RhinoOperations
{
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Rhino.Etl.Core.Operations;

    internal class StagingBulkInsertOperation : SqlBulkInsertOperation
    {
        private readonly V7StagingTable _stagingTable;

        public StagingBulkInsertOperation(string stageConnectionString, V7StagingTable stagingTable, string schemaName, int timeout)
           : base(
               DatabaseUtils.CreateConnectionStringSettings(stageConnectionString),
               DatabaseUtils.GetQualifiedTableName(schemaName, stagingTable.Name), 
               timeout)
        {
            _stagingTable = stagingTable;
        }

        protected override void PrepareSchema()
        {
            foreach (var c in _stagingTable.Columns)
            {
                Schema[c.Name] = DbTypeMatching.GetClrType(c.SqlDbType);
            }
        }
    }
}
