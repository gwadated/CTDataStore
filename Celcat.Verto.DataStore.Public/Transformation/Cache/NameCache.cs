namespace Celcat.Verto.DataStore.Public.Transformation.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Consolidation;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Staging;

    public class NameCache
    {
        private static readonly object _locker = new object();

        private readonly Dictionary<Entity, ResourceNameCache> _caches;
        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        private readonly TableMappings.TableMappings _mappings;

        public NameCache(string connectionString, int timeoutSecs, TableMappings.TableMappings mappings)
        {
            _caches = new Dictionary<Entity, ResourceNameCache>();
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _mappings = mappings;
        }

        private ResourceNameCache GetCache(Entity et, DataStoreConfiguration c)
        {
            _caches.TryGetValue(et, out var cache);
            if (cache == null)
            {
                lock (_locker)
                {
                    _caches.TryGetValue(et, out cache);
                    if (cache == null)
                    {
                        cache = new ResourceNameCache();
                        _caches.Add(et, cache);
                        Populate(et, cache, c);
                    }
                }
            }

            return cache;
        }

        private void Populate(Entity et, ResourceNameCache cache, DataStoreConfiguration c)
        {
            PopulateFromPublicTable(et, cache);
            PopulateFromStagingTable(et, cache, c);
        }

        private void PopulateFromPublicTable(Entity et, ResourceNameCache cache)
        {
            var idFldName = EntityUtils.GetIdFldName(et);
            var stagingTableName = EntityUtils.ToCtTableName(et);

            var mapping = _mappings.FirstOrDefault(
               x => x.PublicStagingTable.Name.Equals(stagingTableName, StringComparison.OrdinalIgnoreCase));
            if (mapping == null)
            {
                throw new ApplicationException($"Could not map staging table ({stagingTableName}) to public table");
            }

            var tableName = DatabaseUtils.GetQualifiedTableName(mapping.PublicTable.SchemaName, mapping.PublicTable.Name);
            PopulateFromTable(et, cache, tableName, idFldName, false);
        }

        private void PopulateFromStagingTable(Entity et, ResourceNameCache cache, DataStoreConfiguration c)
        {
            var idFldName = c.Consolidation.Get(et).None
               ? EntityUtils.GetFederatedFieldName(EntityUtils.GetIdFldName(et))
               : ConsolidationTypeUtils.GetConsolidatedFieldName(EntityUtils.GetIdFldName(et));

            var tableName = DatabaseUtils.GetQualifiedTableName(
                PublicStagingSchema.StagingSchemaName, EntityUtils.ToCtTableName(et));

            PopulateFromTable(et, cache, tableName, idFldName, true);
        }

        private void PopulateFromTable(
            Entity et, ResourceNameCache cache, string qualifiedTableName, string idFldName, bool checkHistoryCols)
        {
            var hasUniqueNameCol = EntityUtils.HasUniqueNameColumn(et);

            var s = new SqlBuilder();
            s.Append(hasUniqueNameCol ? "select name, unique_name" : "select name");
            s.AppendFormat(", {0}", idFldName);
            s.AppendFormat("from {0}", qualifiedTableName);

            if (checkHistoryCols)
            {
                s.AppendFormat(
                    "where {0} in ('{1}', '{2}')", 
                    HistorySchema.HistoryStatusColumnName,
                    HistorySchema.HistoryStatusInsert, 
                    HistorySchema.HistoryStatusUpdate);
            }

            DatabaseUtils.EnumerateResults(_connectionString, s.ToString(), _timeoutSecs, r =>
            {
                var id = (long)DatabaseUtils.SafeRead(r, idFldName, 0L);
                if (id > 0)
                {
                    var name = (string)DatabaseUtils.SafeRead(r, "name", null);
                    string uname = null;

                    if (hasUniqueNameCol)
                    {
                        uname = (string)DatabaseUtils.SafeRead(r, "unique_name", null);
                    }

                    cache.Add(id, name, uname);
                }
            });
        }

        public ResourceName Get(Entity et, long id, DataStoreConfiguration c)
        {
            return GetCache(et, c).Get(id);
        }

        public ResourceName Get(ConsolidationType cType, long id, DataStoreConfiguration c)
        {
            var et = ConsolidationTypeUtils.ToEntity(cType);
            return Get(et, id, c);
        }
    }
}
