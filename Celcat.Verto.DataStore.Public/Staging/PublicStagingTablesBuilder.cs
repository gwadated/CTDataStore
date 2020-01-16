namespace Celcat.Verto.DataStore.Public.Staging
{
    using System;
    using System.Runtime.Caching;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class PublicStagingTablesBuilder : StagingTablesBuilder
    {
        private const int CacheLifetimeMins = 120;
        private const string StagingIdColName = "staging_id";

        private static readonly MemoryCache _memoryCache = new MemoryCache("PublicStagingTablesCache");

        private PublicStagingTablesBuilder()
           : base(PublicStagingSchema.StagingSchemaName)
        {
            foreach (var t in GetTables())
            {
                InsertIdentityColumn(t);

                ((V7StagingTable)t).AddFederatedCols();
                ((V7StagingTable)t).AddConsolidatedCols();

                AddHistoryColumns(t);
                AddPrimaryKey(t);
            }
        }

        private static void AddPrimaryKey(Table t)
        {
            t.AddPrimaryKey(StagingIdColName);
        }

        private static void InsertIdentityColumn(Table t)
        {
            t.PrefixColumn(new IdColumn(StagingIdColName, ColumnNullable.False, true));
        }

        private static void AddHistoryColumns(Table t)
        {
            // indicates the status of the change - (I)nsert, (U)pdate or (D)elete 
            t.AddColumn(new FixedCharColumn(HistorySchema.HistoryStatusColumnName, 1));

            // the date stamp of the change (i.e. when it was inserted into the admin database)
            t.AddColumn(new DateTimeColumn(HistorySchema.HistoryStampColumnName));

            // the date stamp of the change (i.e. when it was inserted into the public database)
            t.AddColumn(new DateTimeColumn(PublicStagingSchema.HistoryStampPublicColumnName));

            // the id of the process (as recorded in the LOG table)
            t.AddColumn(new BigIntColumn(HistorySchema.HistoryLogColumnName));
        }

        public static PublicStagingTablesBuilder Get()
        {
            PublicStagingTablesBuilder result;

            var o = _memoryCache.Get("builder");
            if (o == null)
            {
                result = new PublicStagingTablesBuilder();
                _memoryCache.Set("builder", result, DateTimeOffset.UtcNow.AddMinutes(CacheLifetimeMins));
            }
            else
            {
                result = (PublicStagingTablesBuilder)o;
            }

            return result;
        }
    }
}
