namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7NameColumn : StringColumn
    {
        public Ct7NameColumn(string colName = "name", ColumnNullable nullable = ColumnNullable.True)
           : base(colName, ColumnConstants.StrLenStd, nullable)
        {
        }
    }
}
