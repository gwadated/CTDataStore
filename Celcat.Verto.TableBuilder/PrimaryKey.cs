namespace Celcat.Verto.TableBuilder
{
    using Celcat.Verto.Common;

    public class PrimaryKey : TableKey, IDatabaseObject
    {
        private const string PREFIX = "PK_";

        internal Table Table { private get; set; }

        public bool Clustered { get; set; }

        public string Name { get; set; }

        public PrimaryKey(params TableKeyPart[] keyParts)
           : base(keyParts)
        {
            Clustered = true;
        }

        public PrimaryKey(string colName)
           : base(new TableKeyPart(colName))
        {
            Clustered = true;
        }

        string IDatabaseObject.GenerateSqlToCreate()
        {
            var sb = new SqlBuilder();

            if (Table != null)
            {
                string keyName = Name ?? string.Concat(PREFIX, Table.Name.ToUpper());

                sb.AppendFormat(
                    "ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY",
                    DatabaseUtils.GetQualifiedTableName(Table.SchemaName, Table.Name),
                    DatabaseUtils.EscapeDbObject(keyName));

                if (Clustered)
                {
                    sb.Append("CLUSTERED");
                }

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
