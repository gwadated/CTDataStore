namespace Celcat.Verto.TableBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Text;
    using Celcat.Verto.Common;
    using Celcat.Verto.TableBuilder.Columns;

    public class Table : IDatabaseObject
    {
        private const string Space = " ";

        private string _schemaName;
        private List<TableColumn> _columns;
        private PrimaryKey _primaryKey;
        private List<ForeignKey> _foreignKeyConstraints;
        private List<CheckConstraint> _checkConstraints;
        private List<DefaultValue> _defaultValues;
        private List<Index> _indexes;
        private List<ColumnReferenceCheck> _colRefChecks;

        /// <summary>
        /// Determines if the table is to be created
        /// </summary>
        private bool ShouldCreate { get; set; }

        protected Table(string tableName, string schemaName = DatabaseUtils.StdSchemaName)
        {
            Init(tableName, schemaName);
        }

        /// <summary>
        /// Creates a table based on columns found in specified source table
        /// </summary>
        public Table(string tableName, Table tableToCopyColumnsFrom, string schemaName = DatabaseUtils.StdSchemaName)
        {
            Init(tableName, schemaName);

            foreach (var col in tableToCopyColumnsFrom.Columns)
            {
                AddColumn(col);
            }
        }

        private void Init(string tableName, string schemaName)
        {
            Name = tableName;

            _schemaName = schemaName;
            _columns = new List<TableColumn>();
            _foreignKeyConstraints = new List<ForeignKey>();
            _checkConstraints = new List<CheckConstraint>();
            _defaultValues = new List<DefaultValue>();
            _indexes = new List<Index>();
            _colRefChecks = new List<ColumnReferenceCheck>();

            ShouldCreate = true;
        }

        /// <summary>
        /// Creates a table based on the fields in the specified SqlDataReader
        /// </summary>
        /// <param name="tableName">
        /// Name of table to create
        /// </param>
        /// <param name="r">
        /// Reader on which to base column definitons
        /// </param>
        /// <param name="schemaName">
        /// Name of schema to use
        /// </param>
        protected Table(string tableName, SqlDataReader r, string schemaName = DatabaseUtils.StdSchemaName)
        {
            Init(tableName, schemaName);
            FabricateColumns(r);
        }

        /// <summary>
        /// Creates a table based on the fields in the specified DataTable
        /// </summary>
        /// <param name="tableName">
        /// Name of table to create
        /// </param>
        /// <param name="tableDefinition">
        /// Table definition on which to base columns
        /// </param>
        /// <param name="schemaName">
        /// Name of schema to use
        /// </param>
        protected Table(string tableName, DataTable tableDefinition, string schemaName = DatabaseUtils.StdSchemaName)
        {
            Init(tableName, schemaName);
            FabricateColumns(tableDefinition);
        }

        private void FabricateColumns(SqlDataReader r)
        {
            // create columns based on fields in the specified reader
            FabricateColumns(r.GetSchemaTable());
        }

        private void FabricateColumns(DataTable tableDefinition)
        {
            // create columns based on fields in the specified reader
            if (tableDefinition != null)
            {
                foreach (DataRow row in tableDefinition.Rows)
                {
                    string colName = (string)row[SchemaTableColumn.ColumnName];
                    SqlDbType colType = (SqlDbType)(int)row[SchemaTableColumn.ProviderType];
                    Type dataType = (Type)row[SchemaTableColumn.DataType];

                    int colSize = dataType == typeof(string)
                       ? (int)row[SchemaTableColumn.ColumnSize]
                       : 0;

                    bool nullable = (bool)row[SchemaTableColumn.AllowDBNull];

                    AddColumn(new TableColumn(colName, colType, colSize, nullable ? ColumnNullable.True : ColumnNullable.False));
                }
            }
        }

        public bool ColumnExists(string colName)
        {
            return _columns.Exists(c => c.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));
        }

        public Table PrefixColumn(TableColumn col)
        {
            _columns.Insert(0, col);
            return this;
        }

        public Table AddColumn(params TableColumn[] cols)
        {
            foreach (var col in cols)
            {
                _columns.Add(col);
            }

            return this;
        }

        public Table AddPrimaryKey(PrimaryKey primaryKey)
        {
            primaryKey.Table = this;
            _primaryKey = primaryKey;
            return this;
        }

        public Table AddPrimaryKey(params string[] colNames)
        {
            _primaryKey = new PrimaryKey
            {
                Table = this
            };

            foreach (var c in colNames)
            {
                _primaryKey.AddKeyPart(c);
            }

            return this;
        }

        public Table AddIndex(Index idx)
        {
            idx.Table = this;
            _indexes.Add(idx);
            return this;
        }

        public Table AddIndex(string indexName, bool unique, string colName)
        {
            _indexes.Add(new Index(indexName, unique, colName) { Table = this });
            return this;
        }

        public Table AddCheckConstraint(CheckConstraint c)
        {
            c.Table = this;
            _checkConstraints.Add(c);
            return this;
        }

        public Table AddForeignKey(ForeignKey foreignKey)
        {
            foreignKey.Table = this;
            _foreignKeyConstraints.Add(foreignKey);
            return this;
        }

        public Table AddDefaultValue(DefaultValue v)
        {
            v.Table = this;
            _defaultValues.Add(v);
            return this;
        }

        public Table AddColumnReferenceCheck(ColumnReferenceCheck crc)
        {
            _colRefChecks.Add(crc);
            return this;
        }

        public string SchemaName => _schemaName;

        public IReadOnlyList<TableColumn> Columns => _columns;

        public IReadOnlyList<ColumnReferenceCheck> ColumnReferenceChecks => _colRefChecks;

        public string ColumnsAsCsv
        {
            get
            {
                var sb = new StringBuilder();

                for (var n = 0; n < _columns.Count; ++n)
                {
                    if (n > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append(_columns[n].Name);
                }

                return sb.ToString();
            }
        }

        public string Name { get; set; }

        public string QualifiedName => DatabaseUtils.GetQualifiedTableName(SchemaName, Name);

        public PrimaryKey PrimaryKey => _primaryKey;

        public IEnumerable<Index> Indexes => _indexes;

        string IDatabaseObject.GenerateSqlToCreate()
        {
            var sb = new SqlBuilder();

            if (ShouldCreate)
            {
                sb.AppendFormat("CREATE TABLE {0}", DatabaseUtils.GetQualifiedTableName(_schemaName, Name));
                sb.Append("(");

                for (int n = 0; n < _columns.Count; ++n)
                {
                    if (n > 0)
                    {
                        sb.Append(",");
                    }

                    var col = _columns[n];
                    sb.Append(string.Concat(Space, col.ColumnDefinitionString));
                }

                sb.Append(");");
            }

            if (_primaryKey != null)
            {
                sb.Append(((IDatabaseObject)_primaryKey).GenerateSqlToCreate());
            }

            foreach (var fk in _foreignKeyConstraints)
            {
                sb.Append(((IDatabaseObject)fk).GenerateSqlToCreate());
            }

            foreach (var ck in _checkConstraints)
            {
                sb.Append(((IDatabaseObject)ck).GenerateSqlToCreate());
            }

            foreach (var df in _defaultValues)
            {
                sb.Append(((IDatabaseObject)df).GenerateSqlToCreate());
            }

            foreach (var idx in _indexes)
            {
                sb.Append(((IDatabaseObject)idx).GenerateSqlToCreate());
            }

            return sb.ToString();
        }
    }
}
