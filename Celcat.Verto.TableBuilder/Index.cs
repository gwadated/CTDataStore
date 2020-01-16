namespace Celcat.Verto.TableBuilder
{
    using Celcat.Verto.Common;

    public class Index : TableKey, IDatabaseObject
    {
        private readonly bool _unique;

        internal Table Table { private get; set; }
        
        public string Name { get; set; }

        public Index(string name, bool unique, params TableKeyPart[] keyParts)
           : base(keyParts)
        {
            Name = name;
            _unique = unique;
        }

        public Index(string name, bool unique, string colName)
           : base(new TableKeyPart(colName))
        {
            Name = name;
            _unique = unique;
        }

        public bool IsUnique => _unique;

        public string GenerateSqlToCreate()
        {
            var sb = new SqlBuilder();

            if (Table != null)
            {
                sb.AppendFormat(
                   _unique ? "CREATE UNIQUE NONCLUSTERED INDEX {0} ON {1}" : "CREATE NONCLUSTERED INDEX {0} ON {1}",
                   DatabaseUtils.EscapeDbObject(Name),
                   DatabaseUtils.GetQualifiedTableName(Table.SchemaName, Table.Name));

                sb.Append("(");

                for (int n = 0; n < Key.Count; ++n)
                {
                    if (n > 0)
                    {
                        sb.Append(",");
                    }

                    var part = Key[n];
                    sb.AppendFormat(
                        " {0} {1}",
                        DatabaseUtils.EscapeDbObject(part.ColumnName),
                        Utils.ColumnOrderToSqlString(part.ColumnOrder));
                }

                sb.Append(");");
            }

            return sb.ToString();
        }
    }
}
