namespace Celcat.Verto.DataStore.Public.Staging.RhinoOperations
{
    using System;
    using System.Data;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.History;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    /// <summary>
    /// Gets data from the admin database, history schema
    /// as part of the Etl process to transfer it to the public 
    /// database, staging schema
    /// </summary>
    internal class PublicStagingGetDataOperation : InputCommandOperation
    {
        private readonly string _tableName;

        public PublicStagingGetDataOperation(string adminConnectionString, string tableName)
           : base(DatabaseUtils.CreateConnectionStringSettings(adminConnectionString))
        {
            _tableName = tableName;
        }

        protected override Row CreateRowFromReader(IDataReader reader)
        {
            var result = Row.FromReader(reader);
            result[PublicStagingSchema.HistoryStampPublicColumnName] = DateTime.Now;
            return result;
        }

        protected override void PrepareCommand(IDbCommand cmd)
        {
            // e.g.
            // select * from HISTORY.CT_DEPT where history_federated = 1 and history_applied = 0
            cmd.CommandText =
                $"select * from {DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, _tableName)} where {HistorySchema.HistoryFederatedColumnName} = 1 and {HistorySchema.HistoryAppliedColumnName} = 0";
        }
    }
}
