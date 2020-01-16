namespace Celcat.Verto.TableBuilder.Columns
{
    public class BigIdColumn : BigIntColumn
    {
        public BigIdColumn(string name, bool identity)
           : base(name, ColumnNullable.False, identity)
        {
        }
    }
}
