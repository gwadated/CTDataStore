namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class NullStringColumn : StringColumn
    {
        public NullStringColumn(string name)
           : base(name, ColumnConstants.StrLenStd, ColumnNullable.True)
        {
        }
    }
}
