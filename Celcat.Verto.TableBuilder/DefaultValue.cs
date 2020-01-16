namespace Celcat.Verto.TableBuilder
{
    using Celcat.Verto.Common;

    public class DefaultValue : IDatabaseObject
    {
        private const string Prefix = "DF_";

        private readonly string _colName;
        private readonly string _defaultValue;

        internal Table Table { private get; set; }

        public DefaultValue(string colName, string defaultValue, string name = null)
        {
            _colName = colName;
            _defaultValue = defaultValue;
            Name = name;
        }

        public DefaultValue(string colName, int defaultValue, string name = null)
        {
            _colName = colName;
            _defaultValue = defaultValue.ToString();
            Name = name;
        }

        public string Name { get; set; }

        public string GenerateSqlToCreate()
        {
            var sb = new SqlBuilder();

            if (Table != null)
            {
                string name = Name ?? string.Concat(Prefix, Table.Name, "_", _colName.ToUpper());

                sb.AppendFormat(
                    "ALTER TABLE {0} ADD CONSTRAINT {1} DEFAULT {2} FOR {3}",
                    DatabaseUtils.GetQualifiedTableName(Table.SchemaName, Table.Name),
                    DatabaseUtils.EscapeDbObject(name),
                    _defaultValue,
                    _colName);

                sb.Append(";");
            }

            return sb.ToString();
        }
    }
}
