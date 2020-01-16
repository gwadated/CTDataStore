namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class GuidColumn : TableColumn
    {
        public GuidColumn(string name, ColumnNullable nullable = ColumnNullable.True)
           : base(name, SqlDbType.UniqueIdentifier, 0, nullable)
        {
        }
    }
}
