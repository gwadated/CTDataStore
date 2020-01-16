namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StaffStaffCatTable : PublicResourceTable
    {
        public StaffStaffCatTable()
           : base("STAFF_STAFFCAT")
        {
            AddColumn(new BigIntColumn("staff_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("staff_cat_id", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn("staff_unique_name"));
            AddColumn(new Ct7NameColumn("staff_name"));
            AddColumn(new NotNullStringColumn("staff_cat_name"));

            AddPrimaryKey("staff_id", "staff_cat_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddStaffIdAndNameMapping(c);
            m.AddStaffCatIdAndNameMapping(c);

            return m;
        }
    }
}
