namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class FacultyIdReferenceCheck : ColumnReferenceCheck
    {
        public FacultyIdReferenceCheck() 
            : base("CT_FACULTY", "faculty_id")
        {
        }
    }
}
