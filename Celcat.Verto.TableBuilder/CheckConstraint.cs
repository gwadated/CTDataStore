namespace Celcat.Verto.TableBuilder
{
    using Celcat.Verto.Common;

    public class CheckConstraint : IDatabaseObject
    {
        private readonly string _body;

        internal Table Table { private get; set; }
        
        public CheckConstraint(string constraintName, string body)
        {
            Name = constraintName;
            _body = body;
        }

        public string Name { get; set; }

        public string GenerateSqlToCreate()
        {
            var sb = new SqlBuilder();

            if (Table != null)
            {
                sb.AppendFormat(
                    "ALTER TABLE {0} ADD CONSTRAINT {1} CHECK ({2})",
                    DatabaseUtils.GetQualifiedTableName(Table.SchemaName, Table.Name),
                    DatabaseUtils.EscapeDbObject(Name),
                    _body);

                sb.Append(";");
            }

            return sb.ToString();
        }
    }
}
