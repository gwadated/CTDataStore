namespace Celcat.Verto.DataStore.Admin.Federation.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    /// <summary>
    /// The Federation tables contain a master Id (as used in the Public store)
    /// and an associated source Id (src timetable + item Id). These tables
    /// effectively assign master Ids to all relevant timetable data
    /// </summary>
    internal class FederationTable : Table
    {
        public FederationTable(string tableName)
           : base(tableName, FederationSchema.FederationSchemaName)
        {
            AddColumn(new BigIdColumn(FederationSchema.MasterIdColName, true));
            AddColumn(new IntColumn(ColumnConstants.SrcTimetableIdColumnName, ColumnNullable.False));
            AddColumn(new BigIntColumn(FederationSchema.ItemIdCol, ColumnNullable.False));

            AddPrimaryKey(FederationSchema.MasterIdColName);

            // unique index on src_timetable_id, item_id
            var indexName = string.Concat("IX_", tableName, "_SRC");
            AddIndex(new Index(
                indexName, 
                true,
                new TableKeyPart(ColumnConstants.SrcTimetableIdColumnName),
                new TableKeyPart(FederationSchema.ItemIdCol)));
        }
    }
}
