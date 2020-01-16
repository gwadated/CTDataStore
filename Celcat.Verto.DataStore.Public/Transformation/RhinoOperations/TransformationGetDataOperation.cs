namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using System.Data;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.TableBuilder;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class TransformationGetDataOperation : InputCommandOperation
    {
        private readonly Table _srcTable;
        private readonly TransformationType _transformationType;
        private readonly int _srcTimetableId;

        public TransformationGetDataOperation(
            string adminConnectionString, Table srcTable, TransformationType transformationType, int srcTimetableId)
           : base(DatabaseUtils.CreateConnectionStringSettings(adminConnectionString))
        {
            _srcTable = srcTable;
            _transformationType = transformationType;
            _srcTimetableId = srcTimetableId;
        }

        protected override Row CreateRowFromReader(IDataReader reader)
        {
            return Row.FromReader(reader);
        }

        protected override void PrepareCommand(IDbCommand cmd)
        {
            var sql = new SqlBuilder();
            sql.AppendFormat("select * from {0} where src_timetable_id={1}", _srcTable.QualifiedName, _srcTimetableId);

            switch (_transformationType)
            {
                case TransformationType.Upsert:
                    sql.AppendFormat(
                        "and {0} in ('{1}', '{2}')", 
                        HistorySchema.HistoryStatusColumnName,
                        HistorySchema.HistoryStatusInsert, 
                        HistorySchema.HistoryStatusUpdate);
                    break;

                case TransformationType.Delete:
                default:
                    sql.AppendFormat(
                        "and {0} = '{1}'", HistorySchema.HistoryStatusColumnName, HistorySchema.HistoryStatusDelete);
                    break;
            }

            cmd.CommandText = sql.ToString();
        }
    }
}
