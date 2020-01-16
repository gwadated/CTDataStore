namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StaffStaffCatTable : V7StagingTable
    {
        public StaffStaffCatTable(string schemaName)
           : base("CT_STAFF_STAFFCAT", schemaName)
        {
            AddColumn(new BigIntColumn("staff_id"));
            AddColumn(new BigIntColumn("staff_cat_id"));

            AddColumnReferenceCheck(new StaffIdReferenceCheck());
            AddColumnReferenceCheck(new StaffCatIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
