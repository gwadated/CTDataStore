namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StudentSupervisorTable : V7StagingTable
    {
        public StudentSupervisorTable(string schemaName)
           : base("CT_STUDENT_SUPERVISOR", schemaName)
        {
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new BigIntColumn("supervisor_id"));

            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new SupervisorIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
