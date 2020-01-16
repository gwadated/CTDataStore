namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7NotesColumn : StringColumn
    {
        public Ct7NotesColumn(string name = "notes")
           : base(name, ColumnConstants.StrLenMax, ColumnNullable.True)
        {
        }
    }
}
