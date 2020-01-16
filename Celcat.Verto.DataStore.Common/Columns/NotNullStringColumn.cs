namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class NotNullStringColumn : StringColumn
    {
        public NotNullStringColumn(string name, int len = ColumnConstants.StrLenStd)
           : base(name, len, ColumnNullable.False)
        {
        }
    }
}
