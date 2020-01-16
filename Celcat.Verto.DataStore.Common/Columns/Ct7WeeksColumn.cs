namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7WeeksColumn : StringColumn
    {
        public Ct7WeeksColumn()
           : base("weeks", ColumnConstants.MaxWeeks, ColumnNullable.True)
        {
        }
    }
}
