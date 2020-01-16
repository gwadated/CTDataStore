namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class StringColumn : TableColumn
    {
        public StringColumn(string name, int length, ColumnNullable nullable = ColumnNullable.True)
           : base(name, SqlDbType.NVarChar, length, nullable)
        {
        }
    }
}
