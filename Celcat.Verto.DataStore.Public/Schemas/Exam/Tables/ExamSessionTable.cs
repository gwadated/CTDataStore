namespace Celcat.Verto.DataStore.Public.Schemas.Exam.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamSessionTable : PublicExamTable
    {
        public ExamSessionTable()
           : base("SESSION")
        {
            AddColumn(new BigIntColumn("session_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(new DateTimeColumn("start_date", ColumnNullable.False));

            AddPrimaryKey("session_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("session_id");
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
