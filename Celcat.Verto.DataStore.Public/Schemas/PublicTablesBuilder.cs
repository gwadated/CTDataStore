namespace Celcat.Verto.DataStore.Public.Schemas
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Celcat.Verto.TableBuilder;

    internal class PublicTablesBuilder<T> : Builder 
        where T : PublicTable
    {
        public PublicTablesBuilder()
        {
            AddTables();
        }

        private static Type[] GetTableTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types.Where(t => t.IsSubclassOf(typeof(T))).ToArray();
        }

        private void AddTables()
        {
            // use reflection to find all PublicTable derivatives...
            var tableTypes = GetTableTypes();

            // create an instance of each table and add to the table builder...
            foreach (var tableType in tableTypes)
            {
                PublicTable t = (PublicTable)Activator.CreateInstance(tableType);
                AddTable(t);
            }
        }
    }
}
