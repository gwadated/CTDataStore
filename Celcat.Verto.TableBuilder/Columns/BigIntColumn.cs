namespace Celcat.Verto.TableBuilder.Columns
{
    using System.Data;
    
    public class BigIntColumn : TableColumn
    {
        public BigIntColumn(string name, ColumnNullable nullable = ColumnNullable.True, bool identity = false)
           : base(name, SqlDbType.BigInt, 0, nullable, identity)
        {
        }
    }
}
