namespace Celcat.Verto.DataStore.Admin.Federation.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    /// <summary>
    /// Consolidation tables contain a master consolidation Id and
    /// a natural key. See also ConsolidationDetailTable
    /// </summary>
    public class ConsolidationTable : Table
    {
        public ConsolidationTable(string tableName, string schemaName)
           : base(tableName, schemaName)
        {
            AddColumn(new BigIdColumn(FederationSchema.ConsolidationIdColName, true));
            AddColumn(new StringColumn(FederationSchema.NaturalKeyColName, ColumnConstants.StrLenNaturalKey));

            AddPrimaryKey(FederationSchema.ConsolidationIdColName);

            // unique index on natural_key
            var indexName = string.Concat("IX_", tableName, "_NATURAL");
            AddIndex(indexName, true, FederationSchema.NaturalKeyColName);
        }
    }
}
