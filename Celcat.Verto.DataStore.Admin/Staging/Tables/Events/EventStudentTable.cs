namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;
    
    internal class EventStudentTable : V7StagingTable
    {
        public EventStudentTable(string schemaName)
           : base("CT_EVENT_STUDENT", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new Ct7WeeksColumn());

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new StudentIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
