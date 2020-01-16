namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7BoolColumn : FixedCharColumn
    {
        public Ct7BoolColumn(string name)
           : base(name, 1, ColumnNullable.True)
        {
        }
    }
}
