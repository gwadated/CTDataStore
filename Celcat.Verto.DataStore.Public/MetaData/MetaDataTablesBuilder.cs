namespace Celcat.Verto.DataStore.Public.MetaData
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Celcat.Verto.DataStore.Public.MetaData.Tables;
    using Celcat.Verto.TableBuilder;

    internal class MetaDataTablesBuilder : Builder
    {
        public MetaDataTablesBuilder()
        {
            AddTables();
        }

        private static Type[] GetControlTableTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types.Where(t => t.IsSubclassOf(typeof(MetaDataTable))).ToArray();
        }

        private void AddTables()
        {
            // use reflection to find all ControlTable derivatives...
            Type[] tableTypes = GetControlTableTypes();

            // create an instance of each table and add to the table builder...
            foreach (var tableType in tableTypes)
            {
                MetaDataTable t = (MetaDataTable)Activator.CreateInstance(tableType);
                AddTable(t);
            }
        }
    }
}
