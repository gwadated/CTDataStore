namespace Celcat.Verto.DataStore.Admin.Control.Tables
{
    using Celcat.Verto.TableBuilder;

    internal class ControlTable : Table
    {
        protected ControlTable(string tableName)
           : base(tableName, ControlSchema.ControlSchemaName)
        {
        }
    }
}
