namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class SupervisorTable : PublicResourceTable
    {
        public SupervisorTable()
           : base("SUPERVISOR")
        {
            AddColumn(new BigIntColumn("supervisor_id", ColumnNullable.False));
            AddColumn(new Ct7NameColumn());
            AddColumn(new StringColumn("business_name", ColumnConstants.StrLenStd));
            AddColumn(new Ct7TelephoneColumn("mobile"));
            AddColumn(new Ct7EmailColumn());
            AddColumn(new BitColumn("can_send_sms"));
            AddColumn(new BitColumn("can_send_email"));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("supervisor_id");
            AddNameIndex();
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Supervisor, "supervisor_id");
            m.AddBooleanMapping("can_send_sms");
            m.AddBooleanMapping("can_send_email");

            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
