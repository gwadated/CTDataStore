namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class BitColumn : TableColumn
    {
        public BitColumn(string name, ColumnNullable nullable = ColumnNullable.False)
           : base(name, SqlDbType.Bit, 0, nullable)
        {
        }
    }
}
