namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class IntColumn : TableColumn
    {
        public IntColumn(string name, ColumnNullable nullable = ColumnNullable.True, bool identity = false)
           : base(name, SqlDbType.Int, 0, nullable, identity)
        {
        }
    }
}
