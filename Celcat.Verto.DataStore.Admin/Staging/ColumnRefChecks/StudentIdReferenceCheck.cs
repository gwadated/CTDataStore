namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class StudentIdReferenceCheck : ColumnReferenceCheck
    {
        public StudentIdReferenceCheck() 
            : base("CT_STUDENT", "student_id")
        {
        }
    }
}
