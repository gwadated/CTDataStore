namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7DescriptionColumn : StringColumn
    {
        public Ct7DescriptionColumn()
           : base("description", ColumnConstants.StrLenDescription, ColumnNullable.True)
        {
        }
    }
}
