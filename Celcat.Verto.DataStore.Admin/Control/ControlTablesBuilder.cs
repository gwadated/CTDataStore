namespace Celcat.Verto.DataStore.Admin.Control
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Celcat.Verto.DataStore.Admin.Control.Tables;
    using Celcat.Verto.TableBuilder;

    internal class ControlTablesBuilder : Builder
    {
        public ControlTablesBuilder()
        {
            AddControlTables();
        }

        private static Type[] GetControlTableTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types.Where(t => t.IsSubclassOf(typeof(ControlTable))).ToArray();
        }

        private void AddControlTables()
        {
            // use reflection to find all ControlTable derivatives...
            var tableTypes = GetControlTableTypes();

            // create an instance of each table and add to the table builder...
            foreach (var tableType in tableTypes)
            {
                var t = (ControlTable)Activator.CreateInstance(tableType);
                AddTable(t);
            }
        }
    }
}
