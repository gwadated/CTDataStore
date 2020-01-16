namespace Celcat.Verto.DataStore.Public.Schemas.Exam.Tables
{
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamSlotTable : PublicExamTable
    {
        public ExamSlotTable()
           : base("SLOT")
        {
            AddColumn(new BigIntColumn("slot_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("session_id", ColumnNullable.False));
            AddColumn(new IntColumn("slot_day", ColumnNullable.False));
            AddColumn(new DateTimeColumn("start_time", ColumnNullable.False));
            AddColumn(new DateTimeColumn("end_time", ColumnNullable.False));

            AddPrimaryKey("slot_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("slot_id");
            m.AddFederatedIdMapping("session_id");
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
