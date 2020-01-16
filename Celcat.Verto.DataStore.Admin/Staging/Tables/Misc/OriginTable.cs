namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Misc
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class OriginTable : V7StagingTable
    {
        public OriginTable(string schemaName)
           : base("CT_ORIGIN", schemaName)
        {
            AddColumn(new BigIntColumn("origin_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new StringColumn("type", ColumnConstants.StrLenStd));
            AddColumn(new DateTimeColumn("last_imported"));

            RegisterFederatedIdCols();
        }
    }
}
