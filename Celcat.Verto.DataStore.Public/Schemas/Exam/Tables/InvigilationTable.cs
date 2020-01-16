namespace Celcat.Verto.DataStore.Public.Schemas.Exam.Tables
{
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class InvigilationTable : PublicExamTable
    {
        public InvigilationTable()
           : base("INVIGILATION")
        {
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("slot_id", ColumnNullable.False));

            AddPrimaryKey("event_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("event_id");
            m.AddFederatedIdMapping("slot_id");

            return m;
        }
    }
}
