namespace Celcat.Verto.DataStore.Public.Schemas.Exam.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamEventTable : PublicExamTable
    {
        public ExamEventTable()
           : base("EXAM_EVENT")
        {
            AddColumn(new BigIntColumn("exam_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("exam_unique_name"));
            AddColumn(new NullStringColumn("exam_name"));

            AddPrimaryKey("exam_id", "event_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddExamIdAndNameMapping();
            m.AddFederatedIdMapping("event_id");

            return m;
        }
    }
}
