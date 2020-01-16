namespace Celcat.Verto.DataStore.Admin.Federation.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    /// <summary>
    /// Stores the consolidation configuration settings that were used during the 
    /// most recent federation process
    /// </summary>
    internal class ConsolidationConfigTable : Table
    {
        public ConsolidationConfigTable()
           : base(FederationSchema.ConsolidationConfig, FederationSchema.FederationSchemaName)
        {
            AddColumn(new StringColumn("entity_type", ColumnConstants.StrLenStd, ColumnNullable.False));
            AddColumn(new StringColumn("column_name", ColumnConstants.StrLenStd, ColumnNullable.False));

            AddPrimaryKey("entity_type");
        }
    }
}
