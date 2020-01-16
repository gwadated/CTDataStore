namespace Celcat.Verto.DataStore.Admin.History
{
    using System;
    using System.Runtime.Caching;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class HistoryTablesBuilder : StagingTablesBuilder
    {
        private const int CacheLifetimeMins = 120;
        private static readonly MemoryCache _memoryCache = new MemoryCache("HistoryTablesCache");

        private HistoryTablesBuilder()
           : base(HistorySchema.HistorySchemaName)
        {
            foreach (var t in GetTables())
            {
                // history tables are similar in structure to the v7 staging
                // tables but include extra columns to store federated Ids and 
                // consolidated Ids...
                ((V7StagingTable)t).AddFederatedCols();
                ((V7StagingTable)t).AddConsolidatedCols();

                // ...and some extra audit columns
                AddHistoryColumns(t);
            }
        }

        private static void AddHistoryColumns(Table t)
        {
            // note that all history tables have these extra columns:

            // indicates the status of the change - (I)nsert, (U)pdate or (D)elete 
            t.AddColumn(new FixedCharColumn(HistorySchema.HistoryStatusColumnName, 1));

            // the date stamp of the change (i.e. when it was inserted into the history table
            t.AddColumn(new DateTimeColumn(HistorySchema.HistoryStampColumnName));

            // the id of the process (as recorded in the LOG table)
            t.AddColumn(new BigIntColumn(HistorySchema.HistoryLogColumnName));

            // flag indicating if the row has been federated (all Ids fixed up)
            t.AddColumn(new BitColumn(HistorySchema.HistoryFederatedColumnName));

            // flag indicating if the row has been transferred to public database
            t.AddColumn(new BitColumn(HistorySchema.HistoryAppliedColumnName));
        }

        public static HistoryTablesBuilder Get()
        {
            HistoryTablesBuilder result;

            var o = _memoryCache.Get("builder");
            if (o == null)
            {
                result = new HistoryTablesBuilder();
                _memoryCache.Set("builder", result, DateTimeOffset.UtcNow.AddMinutes(CacheLifetimeMins));
            }
            else
            {
                result = (HistoryTablesBuilder)o;
            }

            return result;
        }
    }
}
