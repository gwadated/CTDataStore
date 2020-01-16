namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7EmailColumn : StringColumn
    {
        public Ct7EmailColumn()
           : base("email", ColumnConstants.StrLenStd, ColumnNullable.True)
        {
        }
    }
}
