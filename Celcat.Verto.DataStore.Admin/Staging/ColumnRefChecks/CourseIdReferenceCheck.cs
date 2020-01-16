namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class CourseIdReferenceCheck : ColumnReferenceCheck
    {
        public CourseIdReferenceCheck() 
            : base("CT_COURSE", "course_id")
        {
        }
    }
}
