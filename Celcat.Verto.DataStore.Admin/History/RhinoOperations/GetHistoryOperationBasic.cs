namespace Celcat.Verto.DataStore.Admin.History.RhinoOperations
{
    using System;
    using System.Data;
    using Celcat.Verto.Common;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class GetHistoryOperationBasic : InputCommandOperation
    {
        private readonly string _stagingTableName;
        private readonly string _stagingSchemaName;
        private readonly int _timetableId;
        private readonly string _historyStatus;
        private readonly long _logId;
        
        public GetHistoryOperationBasic(
            string connectionString, 
            int timetableId, 
            string stagingTableName,
            string stagingSchemaName, 
            string historyStatus, 
            long logId)
           : base(DatabaseUtils.CreateConnectionStringSettings(connectionString))
        {
            _stagingTableName = stagingTableName;
            _stagingSchemaName = stagingSchemaName;
            _timetableId = timetableId;
            _historyStatus = historyStatus;
            _logId = logId;
        }

        protected override Row CreateRowFromReader(IDataReader reader)
        {
            var result = Row.FromReader(reader);
            result[HistorySchema.HistoryStatusColumnName] = _historyStatus;
            result[HistorySchema.HistoryLogColumnName] = _logId;
            result[HistorySchema.HistoryFederatedColumnName] = 0;
            result[HistorySchema.HistoryAppliedColumnName] = 0;
            result[HistorySchema.HistoryStampColumnName] = DateTime.Now;

            return result;
        }

        protected override void PrepareCommand(IDbCommand cmd)
        {
            cmd.CommandText =
                $"select * from {DatabaseUtils.GetQualifiedTableName(_stagingSchemaName, _stagingTableName)} where src_timetable_id = {_timetableId}";
        }
    }
}
