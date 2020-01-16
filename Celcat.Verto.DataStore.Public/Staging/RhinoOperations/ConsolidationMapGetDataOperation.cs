namespace Celcat.Verto.DataStore.Public.Staging.RhinoOperations
{
    using System.Data;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Federation;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class ConsolidationMapGetDataOperation : InputCommandOperation
    {
        private readonly string _tableName;

        public ConsolidationMapGetDataOperation(string adminConnectionString, string tableName)
           : base(DatabaseUtils.CreateConnectionStringSettings(adminConnectionString))
        {
            _tableName = tableName;
        }

        protected override Row CreateRowFromReader(IDataReader reader)
        {
            return Row.FromReader(reader);
        }

        protected override void PrepareCommand(IDbCommand cmd)
        {
            cmd.CommandText =
                $"select * from {DatabaseUtils.GetQualifiedTableName(FederationSchema.FederationSchemaName, _tableName)}";
        }
    }
}
