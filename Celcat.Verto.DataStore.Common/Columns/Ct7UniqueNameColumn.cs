namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7UniqueNameColumn : StringColumn
    {
        public Ct7UniqueNameColumn(string colName = "unique_name")
           : base(colName, ColumnConstants.StrLenStd, ColumnNullable.True)
        {
        }
    }
}
