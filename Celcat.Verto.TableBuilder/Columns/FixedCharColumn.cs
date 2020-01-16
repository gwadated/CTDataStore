namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class FixedCharColumn : TableColumn
    {
        public FixedCharColumn(string name, int length, ColumnNullable nullable = ColumnNullable.True)
           : base(name, SqlDbType.Char, length, nullable)
        {
        }
    }
}
