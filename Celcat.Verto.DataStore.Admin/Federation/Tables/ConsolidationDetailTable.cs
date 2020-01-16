namespace Celcat.Verto.DataStore.Admin.Federation.Tables
{
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    /// <summary>
    /// Consolidation detail tables contain the list of resources that 
    /// form a single consolidated resource based on a a natural key. 
    /// See also ConsolidationTable
    /// </summary>
    public class ConsolidationDetailTable : Table
    {
        public ConsolidationDetailTable(
            string tableName, 
            string masterTable, 
            string schemaName, 
            bool includeForeignKeyConstraints = true)
           : base(tableName, schemaName)
        {
            AddColumn(new BigIdColumn(FederationSchema.ConsolidationIdColName, false));
            AddColumn(new BigIdColumn(FederationSchema.MasterIdColName, false));

            AddPrimaryKey(FederationSchema.ConsolidationIdColName, FederationSchema.MasterIdColName);

            // index on ctds_id
            var indexName = string.Concat("IX_", tableName, "_CTDS_ID");
            AddIndex(indexName, true, FederationSchema.MasterIdColName);

            if (includeForeignKeyConstraints)
            {
                var ct = new ConsolidationTable(masterTable, FederationSchema.FederationSchemaName);
                var fk = new ForeignKey(ct);
                fk.AddReference(FederationSchema.ConsolidationIdColName);
                fk.OnDeleteCascade();
                AddForeignKey(fk);
            }
        }
    }
}
