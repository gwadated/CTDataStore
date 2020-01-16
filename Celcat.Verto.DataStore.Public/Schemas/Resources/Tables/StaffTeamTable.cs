namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StaffTeamTable : PublicResourceTable
    {
        public StaffTeamTable()
           : base("STAFF_TEAM")
        {
            AddColumn(new BigIntColumn("team_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("staff_id", ColumnNullable.False));

            AddColumn(new NotNullStringColumn("team_unique_name"));
            AddColumn(new NullStringColumn("team_name"));
            AddColumn(new NotNullStringColumn("staff_unique_name"));
            AddColumn(new NullStringColumn("staff_name"));

            AddPrimaryKey("team_id", "staff_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddTeamIdAndNameMapping(c);
            m.AddStaffIdAndNameMapping(c);

            return m;
        }
    }
}
