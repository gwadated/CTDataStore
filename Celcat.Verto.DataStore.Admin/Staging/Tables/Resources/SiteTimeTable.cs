namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class SiteTimeTable : V7StagingTable
    {
        public SiteTimeTable(string schemaName)
           : base("CT_SITE_TIME", schemaName)
        {
            AddColumn(new BigIntColumn("site_id1"));
            AddColumn(new BigIntColumn("site_id2"));
            AddColumn(new IntColumn("travel_time"));

            AddColumnReferenceCheck(new ColumnReferenceCheck("CT_SITE", new[] { "site_id" }, new[] { "site_id1" }));
            AddColumnReferenceCheck(new ColumnReferenceCheck("CT_SITE", new[] { "site_id" }, new[] { "site_id2" }));

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
