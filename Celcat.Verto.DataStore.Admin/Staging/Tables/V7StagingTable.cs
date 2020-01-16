namespace Celcat.Verto.DataStore.Admin.Staging.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Federation;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Consolidation;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    public abstract class V7StagingTable : Table
    {
        private readonly List<FederationDefinition> _federatedIdCols;
        private readonly List<ConsolidationDefinition> _consolidatedIdCols;

        protected V7StagingTable(string tableName, string schemaName = DatabaseUtils.StdSchemaName)
           : base(tableName, schemaName)
        {
            _federatedIdCols = new List<FederationDefinition>();
            _consolidatedIdCols = new List<ConsolidationDefinition>();
            AddColumn(new IntColumn(ColumnConstants.SrcTimetableIdColumnName));
        }

        private V7StagingTable(string tableName, SqlDataReader r, string schemaName = DatabaseUtils.StdSchemaName)
           : base(tableName, r, schemaName)
        {
            AddColumn(new IntColumn(ColumnConstants.SrcTimetableIdColumnName));
        }

        private V7StagingTable(string tableName, DataTable tableDefinition, string schemaName = DatabaseUtils.StdSchemaName)
           : base(tableName, tableDefinition, schemaName)
        {
            AddColumn(new IntColumn(ColumnConstants.SrcTimetableIdColumnName));
        }

        public IEnumerable<FederationDefinition> FederatedIdCols => _federatedIdCols;

        public IEnumerable<ConsolidationDefinition> ConsolidatedIdCols => _consolidatedIdCols;

        protected void RegisterFederatedIdCols()
        {
            int count = 0;
            foreach (var c in Columns)
            {
                if (RegisterFederatedIdCol(c.Name))
                {
                    ++count;
                }
            }

            count += RegisterFederatedResourceIdCols();

            if (count == 0)
            {
                throw new ApplicationException(string.Format("No federated columns identified: {0}", Name));
            }
        }

        private int RegisterFederatedResourceIdCols()
        {
            int count = 0;

            const int MAX_RESOURCE_ID_COLS = 3;

            for (int n = 0; n < MAX_RESOURCE_ID_COLS; ++n)
            {
                string resIdCol = string.Format("resource_id{0}", n > 0 ? n.ToString() : string.Empty);
                string resTypeCol = string.Format("resource_type{0}", n > 0 ? n.ToString() : string.Empty);

                if (ColumnExists(resIdCol) && ColumnExists(resTypeCol))
                {
                    RegisterFederatedIdCol(resIdCol, Entity.Unknown, resTypeCol);
                    ++count;
                }
            }

            return count;
        }

        private bool RegisterFederatedIdCol(string colName)
        {
            var e = EntityUtils.GetEntityFromIdFldName(colName);
            
            if (e != Entity.Unknown)
            {
                RegisterFederatedIdCol(colName, e);
                return true;
            }

            return false;
        }

        protected void RegisterFederatedIdCol(string colName, Entity e, string entityDefColName = null)
        {
            if (!ColumnExists(colName))
            {
                throw new ApplicationException(string.Format("Column does not exist: {0}", colName));
            }

            var def = new FederationDefinition
            {
                Entity = e,
                OriginalColName = colName,
                FederationIdColName = EntityUtils.GetFederatedFieldName(colName),
                EntityDefinitionColName = entityDefColName
            };

            _federatedIdCols.Add(def);
        }

        protected void RegisterConsolidatedIdCols()
        {
            var count = 0;

            foreach (var c in Columns)
            {
                if (RegisterConsolidatedIdCol(c.Name))
                {
                    ++count;
                }
            }

            count += RegisterConsolidatedResourceIdCols();

            if (count == 0)
            {
                throw new ApplicationException(string.Format("No consolidated columns identified: {0}", Name));
            }
        }

        private int RegisterConsolidatedResourceIdCols()
        {
            int count = 0;

            const int maxResourceIdCols = 3;

            for (int n = 0; n < maxResourceIdCols; ++n)
            {
                string resIdCol = $"resource_id{(n > 0 ? n.ToString() : string.Empty)}";
                string resTypeCol = $"resource_type{(n > 0 ? n.ToString() : string.Empty)}";

                if (ColumnExists(resIdCol) && ColumnExists(resTypeCol))
                {
                    RegisterConsolidatedIdCol(resIdCol, ConsolidationType.None, resTypeCol);
                    ++count;
                }
            }

            return count;
        }

        private bool RegisterConsolidatedIdCol(string colName)
        {
            var ctype = ConsolidationTypeUtils.GetConsolidationTypeFromIdFldName(colName);
            if (ctype != ConsolidationType.None)
            {
                RegisterConsolidatedIdCol(colName, ctype);
                return true;
            }

            return false;
        }

        private void RegisterConsolidatedIdCol(string colName, ConsolidationType ctype, string entityDefColName = null)
        {
            if (!ColumnExists(colName))
            {
                throw new ApplicationException(string.Format("Column does not exist: {0}", colName));
            }

            var def = new ConsolidationDefinition
            {
                ConsolidationType = ctype,
                OriginalColName = colName,
                ConsolidationIdColName = ConsolidationTypeUtils.GetConsolidatedFieldName(colName),
                EntityDefinitionColName = entityDefColName
            };

            _consolidatedIdCols.Add(def);
        }

        public void AddFederatedCols()
        {
            foreach (var c in _federatedIdCols)
            {
                AddColumn(new BigIntColumn(c.FederationIdColName));
            }
        }

        public void AddConsolidatedCols()
        {
            foreach (var c in _consolidatedIdCols)
            {
                AddColumn(new BigIntColumn(c.ConsolidationIdColName));
            }
        }

        public IEnumerable<string> ColumnsExcludingSrcTimetableColumn
        {
            get
            {
                var result = new List<string>();

                for (var n = 1; n < Columns.Count; ++n)
                {
                    result.Add(Columns[n].Name);
                }

                return result;
            }
        }

        public string ColumnsAsCsvExcludingSrcTimetableColumn => string.Join(",", ColumnsExcludingSrcTimetableColumn);
    }
}
