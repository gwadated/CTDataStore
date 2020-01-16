namespace Celcat.Verto.TableBuilder.Columns
{
    public class IdColumn : IntColumn
    {
        public IdColumn(string name, ColumnNullable nullable = ColumnNullable.False, bool identity = false)
           : base(name, nullable, identity)
        {
        }
    }
}
