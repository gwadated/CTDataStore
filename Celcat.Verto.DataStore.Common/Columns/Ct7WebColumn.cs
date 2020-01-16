namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7WebColumn : StringColumn
    {
        public Ct7WebColumn()
           : base("www", ColumnConstants.StrLenWeb, ColumnNullable.True)
        {
        }
    }
}
