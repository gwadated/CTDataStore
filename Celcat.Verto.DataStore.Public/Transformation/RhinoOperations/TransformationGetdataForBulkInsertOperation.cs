namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using System;
    using System.Data;
    using System.Reflection;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class TransformationGetDataForBulkInsertOperation : InputCommandOperation
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Table _srcTable;
        private readonly PublicTable _targetTable;
        private readonly FixupCaches _caches;
        private readonly DataStoreConfiguration _configuration;
        private readonly TableColumnMappings _colMappings;
        private readonly int _srcTimetableId;

        public TransformationGetDataForBulkInsertOperation(
            string adminConnectionString, 
            Table srcTable, 
            PublicTable targetTable,
            FixupCaches caches, 
            DataStoreConfiguration configuration, 
            int srcTimetableId)
           : base(DatabaseUtils.CreateConnectionStringSettings(adminConnectionString))
        {
            _srcTable = srcTable;
            _targetTable = targetTable;
            _caches = caches;
            _configuration = configuration;
            _colMappings = targetTable.GetColumnMappingsFromStage(_configuration);
            _srcTimetableId = srcTimetableId;
        }

        protected override Row CreateRowFromReader(IDataReader reader)
        {
            var result = Row.FromReader(reader);

            if (_colMappings != null)
            {
                for (int n = 0; n < _colMappings.Count; ++n)
                {
                    var cm = _colMappings[n];

                    if (_colMappings.EventExpansion == null ||
                          (!cm.PublicColumn.Equals(_colMappings.EventExpansion.PublicWeekColumn) &&
                          !cm.PublicColumn.Equals(_colMappings.EventExpansion.PublicEventInstanceColumn) &&
                          !cm.PublicColumn.Equals(_colMappings.EventExpansion.PublicWeekOccurrenceColumn) &&
                          !cm.PublicColumn.Equals("start_time") &&
                          !cm.PublicColumn.Equals("end_time")))
                    {
                        var val = cm.GetStagingValue(result, _caches, _configuration);

                        if (val == null || val == DBNull.Value)
                        {
                            result[cm.PublicColumn] = DBNull.Value;
                        }
                        else
                        {
                            result[cm.PublicColumn] = val;
                        }
                    }
                }
            }

            return result;
        }

        protected override void PrepareCommand(IDbCommand cmd)
        {
            var sql = new SqlBuilder();
            sql.AppendFormat("select * from {0} where src_timetable_id={1}", _srcTable.QualifiedName, _srcTimetableId);

            sql.AppendFormat(
                "and {0} in ('{1}', '{2}')", 
                HistorySchema.HistoryStatusColumnName,
                HistorySchema.HistoryStatusInsert, 
                HistorySchema.HistoryStatusUpdate);

            cmd.CommandText = sql.ToString();
        }
    }
}
