namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class FixedNCharColumn : TableColumn
    {
        public FixedNCharColumn(string name, int length, ColumnNullable nullable = ColumnNullable.True)
           : base(name, SqlDbType.NChar, length, nullable)
        {
        }
    }
}
