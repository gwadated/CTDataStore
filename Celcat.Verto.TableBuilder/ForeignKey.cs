namespace Celcat.Verto.TableBuilder
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Celcat.Verto.Common;

    public class ForeignKey : IDatabaseObject
    {
        private const string Prefix = "FK_";

        private readonly Table _primaryKeyTable;
        private readonly List<string> _foreignKeyColumns;
        private readonly List<string> _primaryKeyColumns;
        private readonly List<string> _rules;

        internal Table Table { private get; set; }

        public string Name { get; set; }

        public ForeignKey(Table primaryKeyTable, params ColumnPair[] cols)
        {
            _primaryKeyTable = primaryKeyTable;
            _foreignKeyColumns = new List<string>();
            _primaryKeyColumns = new List<string>();
            _rules = new List<string>();

            foreach (var c in cols)
            {
                if (string.IsNullOrEmpty(c.ForeignKey))
                {
                    c.ForeignKey = c.PrimaryKey;
                }

                if (!string.IsNullOrEmpty(c.ForeignKey))
                {
                    AddReference(c.ForeignKey, c.PrimaryKey);
                }
            }
        }

        private void AddRule(string rule)
        {
            _rules.Add(rule);
        }

        public void OnDeleteCascade()
        {
            AddRule("ON DELETE CASCADE");
        }

        public void OnDeleteSetNull()
        {
            AddRule("ON DELETE SET NULL");
        }

        public void OnDeleteSetDefault()
        {
            AddRule("ON DELETE SET DEFAULT");
        }

        public void OnUpdateCascade()
        {
            AddRule("ON UPDATE CASCADE");
        }

        public void OnUpdateSetNull()
        {
            AddRule("ON UPDATE SET NULL");
        }

        public void OnUpdateSetDefault()
        {
            AddRule("ON UPDATE SET DEFAULT");
        }

        public ForeignKey AddReference(string foreignKeyCol, string primaryKeyCol = null)
        {
            _foreignKeyColumns.Add(foreignKeyCol);
            _primaryKeyColumns.Add(primaryKeyCol ?? foreignKeyCol);
            return this;
        }

        string IDatabaseObject.GenerateSqlToCreate()
        {
            var sb = new SqlBuilder();

            if (Table != null)
            {
                var keyName = Name ?? string.Concat(Prefix, Table.Name.ToUpper(), "_", _primaryKeyTable.Name.ToUpper());

                sb.AppendFormat(
                    "ALTER TABLE {0} ADD CONSTRAINT {1}",
                    DatabaseUtils.GetQualifiedTableName(Table.SchemaName, Table.Name),
                    DatabaseUtils.EscapeDbObject(keyName));

                sb.AppendFormat("FOREIGN KEY ({0})", ColsAsCsv(_foreignKeyColumns));

                sb.AppendFormat(
                    "REFERENCES {0} ({1})",
                    DatabaseUtils.GetQualifiedTableName(_primaryKeyTable.SchemaName, _primaryKeyTable.Name),
                    ColsAsCsv(_primaryKeyColumns));

                if (_rules.Any())
                {
                    foreach (var rule in _rules)
                    {
                        sb.Append(rule);
                    }
                }

                sb.Append(";");
            }

            return sb.ToString();
        }

        private string ColsAsCsv(IReadOnlyList<string> colNames)
        {
            var sb = new StringBuilder();

            for (int n = 0; n < colNames.Count; ++n)
            {
                if (n > 0)
                {
                    sb.Append(", ");
                }

                sb.AppendFormat("{0}", DatabaseUtils.EscapeDbObject(colNames[n]));
            }

            return sb.ToString();
        }
    }
}
