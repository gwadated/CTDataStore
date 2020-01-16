namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7SexColumn : FixedCharColumn
    {
        public Ct7SexColumn()
           : base("sex", 1, ColumnNullable.True)
        {
        }
    }
}
