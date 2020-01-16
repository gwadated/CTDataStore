namespace Celcat.Verto.DataStore.Public.MetaData.Tables
{
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class MetaDataConfigTable : MetaDataTable
    {
        public MetaDataConfigTable()
           : base(MetaDataSchema.ConfigTableName)
        {
            // the app_key is used to ensure that Admin and Public databases are paired correctly
            AddColumn(new IdColumn("config_id"));
            AddColumn(new IntColumn("public_database_version", ColumnNullable.False));
            AddColumn(new GuidColumn("app_key", ColumnNullable.False));

            AddPrimaryKey("config_id");
            AddCheckConstraint(new CheckConstraint("CK_CONFIG_ID", "config_id = 1"));
            AddDefaultValue(new DefaultValue("config_id", 1));
        }
    }
}
