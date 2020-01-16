namespace Celcat.Verto.DataStore.Admin.Staging.RhinoOperations
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Columns;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class GetTimetableDataOperation : InputCommandOperation
    {
        private readonly V7StagingTable _targetTable;
        private readonly int _timetableId;

        public GetTimetableDataOperation(string connectionString, V7StagingTable targetTable, int timetableId)
           : base(DatabaseUtils.CreateConnectionStringSettings(connectionString))
        {
            _targetTable = targetTable;
            _timetableId = timetableId;
        }

        protected override Row CreateRowFromReader(IDataReader reader)
        {
            return Row.FromReader(reader);
        }

        protected override void PrepareCommand(IDbCommand cmd)
        {
            if (_targetTable.Name.Equals(StagingTablesBuilder.PseudoRegisterMarkTable, StringComparison.OrdinalIgnoreCase))
            {
                // special case...
                cmd.CommandText = GetRegisterMarkSql();
            }
            else if (_targetTable.Name.Equals("CT_EVENT_CAT", StringComparison.OrdinalIgnoreCase))
            {
                // special case - we need to add "register_req_resolved" column
                cmd.CommandText = GetEventCatSql();
            }
            else if (_targetTable.Name.Equals("CT_EVENT", StringComparison.OrdinalIgnoreCase))
            {
                // special case - we need to add "register_req_resolved" column
                cmd.CommandText = GetEventSql();
            }
            else if (_targetTable.Name.Equals("CT_ES_EXAM", StringComparison.OrdinalIgnoreCase))
            {
                // special case - we need to add "register_req_resolved" column
                cmd.CommandText = GetExamSql();
            }
            else
            {
                cmd.CommandText =
                    $"select {_timetableId} as {ColumnConstants.SrcTimetableIdColumnName}, {_targetTable.ColumnsAsCsvExcludingSrcTimetableColumn} from {_targetTable.Name}";
            }
        }

        private string GetEventCatSql()
        {
            // during staging we resolve the register_req column...
            List<string> cols = _targetTable.ColumnsExcludingSrcTimetableColumn.ToList();
            cols.Remove(ColumnConstants.RegistersReqResolvedColumnName);

            var colNamesCsv = string.Join(",ec.", cols);

            var sql = new SqlBuilder();
            sql.AppendFormat(
                "select {0} as {1}, {2}, {3} = COALESCE(ec.registers_req, c.registers_req)",
                _timetableId,
                ColumnConstants.SrcTimetableIdColumnName,
                colNamesCsv,
                ColumnConstants.RegistersReqResolvedColumnName);

            sql.Append("from CT_EVENT_CAT ec inner join CT_CONFIG c on c.id=1");

            return sql.ToString();
        }

        private string GetEventSql()
        {
            // during staging we resolve the register_req column...

            List<string> cols = _targetTable.ColumnsExcludingSrcTimetableColumn.ToList();
            cols.Remove(ColumnConstants.RegistersReqResolvedColumnName);

            var colNamesCsv = string.Join(",e.", cols);

            var sql = new SqlBuilder();
            sql.AppendFormat(
                "select {0} as {1}, {2}, {3} = COALESCE(COALESCE(e.registers_req, ec.registers_req), c.registers_req)",
                _timetableId,
                ColumnConstants.SrcTimetableIdColumnName,
                colNamesCsv,
                ColumnConstants.RegistersReqResolvedColumnName);

            sql.Append("from CT_EVENT e left outer join CT_EVENT_CAT ec on ec.event_cat_id=e.event_cat_id");
            sql.Append("inner join CT_CONFIG c on c.id=1");

            return sql.ToString();
        }

        private string GetExamSql()
        {
            // during staging we resolve the register_req column...

            var cols = _targetTable.ColumnsExcludingSrcTimetableColumn.ToList();
            cols.Remove(ColumnConstants.RegistersReqResolvedColumnName);

            var colNamesCsv = string.Join(",e.", cols);

            var sql = new SqlBuilder();
            sql.AppendFormat(
                "select {0} as {1}, {2}, {3} = COALESCE(COALESCE(e.registers_req, ec.registers_req), c.registers_req)",
                _timetableId,
                ColumnConstants.SrcTimetableIdColumnName,
                colNamesCsv,
                ColumnConstants.RegistersReqResolvedColumnName);

            sql.Append("from CT_ES_EXAM e left outer join CT_EVENT_CAT ec on ec.event_cat_id=e.event_cat_id");
            sql.Append("inner join CT_CONFIG c on c.id=1");

            return sql.ToString();
        }

        private string GetRegisterMarkSql()
        {
            // during staging we take the opportunity to simplify 
            // attendance data by removing the following tables:
            // CT_AT_EXCEPTION, CT_AT_STUDENT_EXCEPTION, CT_AT_STUDENT_REGISTER and CT_AT_STUDENT_MARK
            // and creating a new table called CT_AT_REGISTER_MARK that combines data from 
            // the tables that are omitted...
            var sql = new SqlBuilder();
            sql.AppendFormat(
                "select {0} as {1}, sm.student_id, sm.event_id, sm.week, sm.mark_id, sm.mins_late,",
                _timetableId,
                ColumnConstants.SrcTimetableIdColumnName);

            sql.Append("sm.comments, sm.source, date_change, user_id_change");
            sql.Append("FROM CT_AT_STUDENT_MARK sm");

            sql.Append("UNION ALL");

            sql.AppendFormat(
                "select {0} as {1}, sr.student_id, sr.event_id, sr.week, e.mark_id, e.mins_late,",
                _timetableId,
                ColumnConstants.SrcTimetableIdColumnName);

            sql.Append("e.comments, e.type, e.date_change, e.user_id_change");
            sql.Append("FROM CT_AT_STUDENT_REGISTER sr");
            sql.Append("LEFT OUTER JOIN CT_AT_EXCEPTION e");
            sql.Append("ON sr.exception_id = e.exception_id");
            sql.Append("WHERE NOT EXISTS");
            sql.Append("(SELECT 1 FROM CT_AT_STUDENT_MARK");
            sql.Append("WHERE CT_AT_STUDENT_MARK.student_id = sr.student_id");
            sql.Append("AND CT_AT_STUDENT_MARK.event_id = sr.event_id");
            sql.Append("AND CT_AT_STUDENT_MARK.week = sr.week)");

            return sql.ToString();
        }
    }
}
