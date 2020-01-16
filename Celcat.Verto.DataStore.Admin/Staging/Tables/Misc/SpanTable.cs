namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Misc
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;
    
    internal class SpanTable : V7StagingTable
    {
        public SpanTable(string schemaName)
           : base("CT_SPAN", schemaName)
        {
            AddColumn(new BigIntColumn("span_id"));
            AddColumn(new BigIntColumn("user_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new Ct7WeeksColumn());
            AddColumn(new Ct7BoolColumn("access"));

            AddColumnReferenceCheck(new ColumnReferenceCheck("CT_USER", "user_id"));

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
