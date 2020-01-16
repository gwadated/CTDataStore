namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class CharColumn : TableColumn
    {
        public CharColumn(string name, int length, ColumnNullable nullable = ColumnNullable.True)
           : base(name, SqlDbType.VarChar, length, nullable)
        {
        }
    }
}
