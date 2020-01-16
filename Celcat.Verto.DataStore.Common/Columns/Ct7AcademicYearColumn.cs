namespace Celcat.Verto.DataStore.Common.Columns
{
    using Celcat.Verto.TableBuilder.Columns;

    public class Ct7AcademicYearColumn : StringColumn
    {
        public Ct7AcademicYearColumn()
           : base("academic_year", ColumnConstants.StrLenAcademicYear, ColumnNullable.True)
        {
        }
    }
}
