namespace Celcat.Verto.DataStore.Admin.Control.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ControlSourceTable : ControlTable
    {
        public ControlSourceTable()
           : base(ControlSchema.SrcTimetableName)
        {
            AddColumn(new IdColumn(ColumnConstants.SrcTimetableIdColumnName, ColumnNullable.False, true));
            AddColumn(new StringColumn("timetable_name", ColumnConstants.StrLenStd, ColumnNullable.False));
            AddColumn(new StringColumn("server_name", ColumnConstants.StrLenStd, ColumnNullable.False));
            AddColumn(new StringColumn("database_name", ColumnConstants.StrLenStd, ColumnNullable.False));
            AddColumn(new IntColumn("schema_version", ColumnNullable.False));
            AddColumn(new GuidColumn("guid", ColumnNullable.False));

            AddPrimaryKey(ColumnConstants.SrcTimetableIdColumnName);
            AddIndex("IX_SRC_TIMETABLE_GUID", true, "guid");
        }
    }
}
