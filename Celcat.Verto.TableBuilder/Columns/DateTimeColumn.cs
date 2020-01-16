namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;

    public class DateTimeColumn : TableColumn
    {
        public DateTimeColumn(string name, ColumnNullable nullable = ColumnNullable.True)
           : base(name, SqlDbType.DateTime, 0, nullable)
        {
        }
    }
}
