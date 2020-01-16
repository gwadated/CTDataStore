namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class FloatColumn : TableColumn
    {
        public FloatColumn(string name, ColumnNullable nullable = ColumnNullable.True)
           : base(name, SqlDbType.Float, 0, nullable)
        {
        }
    }
}
