namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7TelephoneColumn : StringColumn
    {
        public Ct7TelephoneColumn(string name = "tel")
           : base(name, ColumnConstants.StrLenTel, ColumnNullable.True)
        {
        }
    }
}
