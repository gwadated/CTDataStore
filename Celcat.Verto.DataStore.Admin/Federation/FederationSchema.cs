namespace Celcat.Verto.DataStore.Admin.Federation
{
    using System;
    using System.Data.SqlClient;
    using System.Reflection;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Staging;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Common.Schemas;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;

    /// <summary>
    /// Federation is the process of assigning unique CTDS master Ids to timetable entities. e.g.
    /// event 123 extracted from Timetable A may be given Id = 38462; an Id that is unique across 
    /// all events from all timetables in the data store. 
    /// 
    /// </summary>
    public class FederationSchema : SchemaBase
    {
        public const string FederationSchemaName = "FEDERATION";
        public const string MasterIdColName = "ctds_id";
        public const string ConsolidationIdColName = "consolidation_id";
        public const string ItemIdCol = "item_id";
        public const string NaturalKeyColName = "natural_key";
        public const string ConsolidationConfig = "CONSOLIDATION_CONFIG";

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal FederationSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }

        /// <summary>
        /// creates table in the Admin db, FEDERATION schema
        /// </summary>
        public void CreateTables()
        {
            _log.DebugFormat("Creating federation tables in admin database");

            EnsureSchemaCreated();
            new FederationTablesBuilder().Execute(ConnectionString, TimeoutSecs);
            new ConsolidationTablesBuilder().Execute(ConnectionString, TimeoutSecs);
        }

        /// <summary>
        /// Updates the consolidation config table
        /// </summary>
        /// <param name="c">
        /// The new values
        /// </param>
        /// <returns>
        /// True if configuration was changed
        /// </returns>
        public bool UpdateConsolidationConfigTable(ConsolidationParams c)
        {
            var fromDb = ReadConsolidationConfig();

            if (fromDb.DiffersFrom(c))
            {
                _log.Debug("Consolidation configuration has changed since last federation process");
             
                WriteConsolidationConfig(c);
                return true;
            }

            return false;
        }

        public bool ConsolidationConfigChanged(ConsolidationParams c)
        {
            var fromDb = ReadConsolidationConfig();
            return fromDb.DiffersFrom(c);
        }

        public bool ConsolidationConfigTableExists()
        {
            return DatabaseUtils.TableExists(ConnectionString, TimeoutSecs, ConsolidationConfig, SchemaName);
        }

        private void WriteConsolidationConfig(ConsolidationParams c)
        {
            var qualifiedConfigTable = GetQualifiedTableName(ConsolidationConfig);

            var sql = $"delete from {qualifiedConfigTable}";
            DatabaseUtils.ExecuteSql(ConnectionString, sql, TimeoutSecs);
            
            if (c.Enabled)
            {
                foreach (var entry in c.Entries)
                {
                    var e = EntityUtils.FromString(entry.Entity);

                    if (EntityUtils.CanParticipateInConsolidation(e))
                    {
                        sql = $"insert into {qualifiedConfigTable} (entity_type, column_name) values (@E, @C)";

                        SqlParameter[] p = { new SqlParameter("@E", entry.Entity), new SqlParameter("@C", entry.Column) };
                        DatabaseUtils.ExecuteSql(ConnectionString, sql, TimeoutSecs, p);
                    }
                }
            }
        }

        private ConsolidationParams ReadConsolidationConfig()
        {
            var result = new ConsolidationParams();

            var sql = $"select entity_type, column_name from {GetQualifiedTableName(ConsolidationConfig)}";

            DatabaseUtils.EnumerateResults(ConnectionString, sql, TimeoutSecs, r =>
            {
                var e = EntityUtils.FromString((string)r["entity_type"]);

                if (EntityUtils.CanParticipateInConsolidation(e))
                {
                    string colName = (string)DatabaseUtils.SafeRead(r, "column_name", string.Empty);
                    if (!string.IsNullOrEmpty(colName))
                    {
                        var entry = new ConsolidationEntry
                        {
                            Entity = e.ToString(),
                            Column = colName
                        };

                        result.Entries.Add(entry);
                    }
                }
            });

            result.Enabled = result.Entries.Count > 0;

            return result;
        }

        public void UpdateStdFederationTable(Entity entity, Table stagingTable)
        {
            if (stagingTable != null && !stagingTable.Name.Equals("CT_CONFIG", StringComparison.OrdinalIgnoreCase))
            {
                // update Federation tables as required

                // e.g.
                // MERGE FEDERATION.[EVENT] f
                // USING STAGEA.CT_EVENT r
                // on f.src_timetable_id = r.src_timetable_id and f.item_id = r.event_id
                // when NOT MATCHED BY TARGET THEN
                //   insert (src_timetable_id, item_id)
                //   values (r.src_timetable_id, r.event_id)
                // when NOT MATCHED BY SOURCE THEN
                //   DELETE;
                var qualifiedFederationTableName = GetQualifiedTableName(EntityUtils.ToFederationTableName(entity));

                var qualifiedStagingTableName =
                   DatabaseUtils.GetQualifiedTableName(StagingSchema.PrimaryStagingSchemaName, stagingTable.Name);

                var entityIdColumnName = stagingTable.Columns[1].Name;

                _log.DebugFormat("Updating federation table {0}", qualifiedFederationTableName);

                var sql = new SqlBuilder();

                sql.AppendFormat("MERGE {0} f", qualifiedFederationTableName);
                sql.AppendFormat("USING {0} r", qualifiedStagingTableName);
                sql.AppendFormat(
                    "on f.{0}=r.{0} and f.{1}=r.{2}",
                    ColumnConstants.SrcTimetableIdColumnName, 
                    ItemIdCol, 
                    entityIdColumnName);
                sql.Append("WHEN NOT MATCHED BY TARGET THEN");
                sql.AppendFormat("insert ({0}, {1})", ColumnConstants.SrcTimetableIdColumnName, ItemIdCol);
                sql.AppendFormat("values (r.{0}, r.{1})", ColumnConstants.SrcTimetableIdColumnName, entityIdColumnName);
                sql.Append("WHEN NOT MATCHED BY SOURCE THEN");
                sql.Append("DELETE;");

                int numChanges = DatabaseUtils.ExecuteSql(ConnectionString, sql.ToString(), TimeoutSecs);

                _log.DebugFormat("{0} changes to federation table {1}", numChanges, qualifiedFederationTableName);
            }
        }

        public void UpdateConsolidationTables(Entity entity, Table stagingTable, ConsolidationEntry consolidationConfig)
        {
            UpdateConsolidationMasterTables(entity, stagingTable, consolidationConfig);
            UpdateConsolidationDetailTables(entity, stagingTable, consolidationConfig);
        }

        private void UpdateConsolidationMasterTables(Entity entity, Table stagingTable, ConsolidationEntry consolidationConfig)
        {
            string qualifiedConsolidationTableName = GetQualifiedTableName(EntityUtils.ToConsolidationTableName(entity));

            int numChanges;

            if (consolidationConfig.None)
            {
                var sql = $"delete from {qualifiedConsolidationTableName}";
                numChanges = DatabaseUtils.ExecuteSql(ConnectionString, sql, TimeoutSecs);
            }
            else
            {
                string qualifiedFederationTableName = GetQualifiedTableName(EntityUtils.ToFederationTableName(entity));

                string qualifiedStagingTableName =
                   DatabaseUtils.GetQualifiedTableName(StagingSchema.PrimaryStagingSchemaName, stagingTable.Name);

                string entityIdColumnName = stagingTable.Columns[1].Name;

                _log.DebugFormat("Updating consolidation table {0}", qualifiedConsolidationTableName);

                var sql = new SqlBuilder();

                // e.g.
                // MERGE FEDERATION.CONSOLIDATION.DEPT c
                // USING (select distinct r.name from STAGEA.CT_DEPT r
                // inner join FEDERATION.DEPT f
                // on f.src_timetable_id = r.src_timetable_id
                // and f.item_id = r.dept_id) tmpTable
                // on c.natural_key = tmpTable.name
                // when NOT MATCHED BY TARGET THEN
                //   insert (natural_key)
                //   values (tmpTable.name)
                // when NOT MATCHED BY SOURCE THEN
                //   DELETE;
                sql.AppendFormat("MERGE {0} c", qualifiedConsolidationTableName);
                sql.AppendFormat(
                    "USING (select distinct r.{0} from {1} r", consolidationConfig.Column, qualifiedStagingTableName);
                sql.AppendFormat("inner join {0} f", qualifiedFederationTableName);
                sql.AppendFormat("on f.{0} = r.{0}", ColumnConstants.SrcTimetableIdColumnName);
                sql.AppendFormat("and f.{0} = r.{1}) tmpTable", ItemIdCol, entityIdColumnName);
                sql.AppendFormat("on c.{0} = tmpTable.{1}", NaturalKeyColName, consolidationConfig.Column);
                sql.Append("when NOT MATCHED BY TARGET THEN");
                sql.AppendFormat("insert ({0})", NaturalKeyColName);
                sql.AppendFormat("values (tmpTable.{0})", consolidationConfig.Column);
                sql.Append("when NOT MATCHED BY SOURCE THEN");
                sql.Append("DELETE;");

                numChanges = DatabaseUtils.ExecuteSql(ConnectionString, sql.ToString(), TimeoutSecs);
            }

            _log.DebugFormat("{0} changes to consolidation table {1}", numChanges, qualifiedConsolidationTableName);
        }

        private void UpdateConsolidationDetailTables(Entity entity, Table stagingTable, ConsolidationEntry consolidationConfig)
        {
            string qualifiedConsolidationDetailTableName = GetQualifiedTableName(EntityUtils.ToConsolidationDetailTableName(entity));

            int numChanges;

            if (consolidationConfig.None)
            {
                var sql = $"delete from {qualifiedConsolidationDetailTableName}";
                numChanges = DatabaseUtils.ExecuteSql(ConnectionString, sql, TimeoutSecs);
            }
            else
            {
                string qualifiedStagingTableName =
                   DatabaseUtils.GetQualifiedTableName(StagingSchema.PrimaryStagingSchemaName, stagingTable.Name);

                string qualifiedConsolidationTableName = GetQualifiedTableName(EntityUtils.ToConsolidationTableName(entity));

                string qualifiedFederationTableName = GetQualifiedTableName(EntityUtils.ToFederationTableName(entity));

                string entityIdColumnName = stagingTable.Columns[1].Name;

                // e.g.
                // MERGE FEDERATION.CONSOLIDATION_DETAIL.DEPT cd
                // USING (select c.consolidation_id, f.ctds_id 
                // from STAGEA.DEPT r
                // inner join FEDERATION.CONSOLIDATION_DEPT c
                // on c.natural_key = r.name
                // inner join FEDERATION.DEPT f
                // on f.src_timetable_id = r.src_timetable_id
                // and f.item_id = r.dept_id) tmpTable
                // on cd.consolidation_id = tmpTable.consolidation_id and cd.ctds_id = tmpTable.ctds_id
                // when NOT MATCHED BY TARGET THEN
                //   insert (consolidation_id, ctds_id)
                //   values (tmpTable.consolidation_id, tmpTable.ctds_id)
                // when NOT MATCHED BY SOURCE THEN
                //   DELETE;
                var sql = new SqlBuilder();

                sql.AppendFormat("MERGE {0} cd", qualifiedConsolidationDetailTableName);
                sql.AppendFormat("USING (select c.{0}, f.{1}", ConsolidationIdColName, MasterIdColName);
                sql.AppendFormat("from {0} r", qualifiedStagingTableName);
                sql.AppendFormat("inner join {0} c", qualifiedConsolidationTableName);
                sql.AppendFormat("on c.{0} = r.{1}", NaturalKeyColName, consolidationConfig.Column);
                sql.AppendFormat("inner join {0} f", qualifiedFederationTableName);
                sql.AppendFormat("on f.{0} = r.{0}", ColumnConstants.SrcTimetableIdColumnName);
                sql.AppendFormat("and f.{0} = r.{1}) tmpTable", ItemIdCol, entityIdColumnName);
                sql.AppendFormat("on cd.{0} = tmpTable.{0} and cd.{1} = tmpTable.{1}", ConsolidationIdColName, MasterIdColName);
                sql.Append("when NOT MATCHED BY TARGET THEN");
                sql.AppendFormat("insert ({0}, {1})", ConsolidationIdColName, MasterIdColName);
                sql.AppendFormat("values (tmpTable.{0}, tmpTable.{1})", ConsolidationIdColName, MasterIdColName);
                sql.Append("when NOT MATCHED BY SOURCE THEN");
                sql.Append("DELETE;");

                numChanges = DatabaseUtils.ExecuteSql(ConnectionString, sql.ToString(), TimeoutSecs);
            }

            _log.DebugFormat("{0} changes to consolidation detail table {1}", numChanges, qualifiedConsolidationDetailTableName);
        }

        protected override string SchemaName => FederationSchemaName;
    }
}
