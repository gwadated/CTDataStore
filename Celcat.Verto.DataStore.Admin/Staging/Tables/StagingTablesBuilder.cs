namespace Celcat.Verto.DataStore.Admin.Staging.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Caching;
    using Celcat.Verto.TableBuilder;

    public class StagingTablesBuilder : Builder
    {
        public const string PseudoRegisterMarkTable = "CT_AT_REGISTER_MARK";
        private const int CacheLifetimeMins = 120;

        private static readonly MemoryCache _memoryCache = new MemoryCache("StagingTablesCache");
        private readonly string _schemaName;

        protected StagingTablesBuilder(string schemaName)
        {
            _schemaName = schemaName;
            AddV7StagingTables();
        }

        private static Type[] GetV7StagingTableTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types.Where(t => t.IsSubclassOf(typeof(V7StagingTable))).ToArray();
        }

        private void AddV7StagingTables()
        {
            // use reflection to find all V7StagingTable derivatives...
            var tableTypes = GetV7StagingTableTypes();

            // create an instance of each table and add to the table builder...
            foreach (var tableType in tableTypes)
            {
                var t = (V7StagingTable)Activator.CreateInstance(tableType, _schemaName);
                AddTable(t);
            }
        }

        public static StagingTablesBuilder Get(string schemaName)
        {
            StagingTablesBuilder result;

            var o = _memoryCache.Get(schemaName);
            if (o == null)
            {
                result = new StagingTablesBuilder(schemaName);
                _memoryCache.Set(schemaName, result, DateTimeOffset.UtcNow.AddMinutes(CacheLifetimeMins));
            }
            else
            {
                result = (StagingTablesBuilder)o;
            }

            return result;
        }

        public IReadOnlyList<string> GetTableNames(bool includePseudoRegisterMarkTable)
        {
            var result = GetTableNames().ToList();

            if (!includePseudoRegisterMarkTable)
            {
                foreach (var t in result)
                {
                    if (t.Equals(PseudoRegisterMarkTable, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Remove(t);
                        break;
                    }
                }
            }

            return result.ToArray();
        }

        public IReadOnlyList<Table> GetTables(bool includePseudoRegisterMarkTable)
        {
            var result = GetTables().ToList();

            if (!includePseudoRegisterMarkTable)
            {
                foreach (var t in result)
                {
                    if (t.Name.Equals(PseudoRegisterMarkTable, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Remove(t);
                        break;
                    }
                }
            }

            return result.ToArray();
        }
    }
}
