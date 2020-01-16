namespace Celcat.Verto.DataStore.Admin.Control.Tables
{
    using System;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ControlLogTable : ControlTable
    {
        public ControlLogTable()
           : base(ControlSchema.LogTableName)
        {
            AddColumn(new BigIdColumn("log_id", true));  // true => identity
            AddColumn(new StringColumn("task", ColumnConstants.StrLenStd, ColumnNullable.False));
            AddColumn(new StringColumn("description", int.MaxValue));
            AddColumn(new DateTimeColumn("stamp", ColumnNullable.False));
            AddDefaultValue(new DefaultValue("stamp", "GETDATE()"));

            AddPrimaryKey("log_id");
        }
    }
}
