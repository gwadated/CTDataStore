namespace Celcat.Verto.DataStore.Admin.Control.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ControlConfigTable : ControlTable
    {
        // the app_key is used to ensure that Admin and Public databases are paired correctly
        public ControlConfigTable()
           : base(ControlSchema.ConfigTableName)
        {
            AddColumn(new IdColumn("config_id"));
            AddColumn(new IntColumn("admin_database_version", ColumnNullable.False));
            AddColumn(new GuidColumn("app_key", ColumnNullable.False));
            AddColumn(new GuidColumn("mutex_value"));
            AddColumn(new StringColumn("mutex_machine_name", ColumnConstants.StrLenStd));
            AddColumn(new DateTimeColumn("mutex_start"));
            AddColumn(new DateTimeColumn("mutex_touched"));

            AddPrimaryKey("config_id");
            AddCheckConstraint(new CheckConstraint("CK_CONFIG_ID", "config_id = 1"));
            AddDefaultValue(new DefaultValue("config_id", 1));
        }
    }
}
