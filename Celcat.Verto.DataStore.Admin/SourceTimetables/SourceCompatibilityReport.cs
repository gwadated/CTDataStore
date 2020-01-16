namespace Celcat.Verto.DataStore.Admin.SourceTimetables
{
    using System.Collections.Generic;
    using System.Text;
    using Celcat.Verto.DataStore.Admin.Resources;

    public class SourceCompatibilityReport
    {
        private readonly string _sourceDescription;
        private readonly List<string> _missingTables;
        private readonly List<string> _extraTables;
        private readonly List<string> _missingColumns;
        private readonly List<string> _extraColumns;
        private readonly List<string> _badDataTypes;

        private bool _isCompatible;

        internal SourceCompatibilityReport(string srcDescription)
        {
            _sourceDescription = srcDescription;
            _isCompatible = true;
            _missingTables = new List<string>();
            _extraTables = new List<string>();
            _missingColumns = new List<string>();
            _extraColumns = new List<string>();
            _badDataTypes = new List<string>();
        }

        public bool IsCompatible => _isCompatible;

        public string SourceDescription => _sourceDescription;

        internal void AddMissingTable(string tableName)
        {
            _isCompatible = false;
            _missingTables.Add(tableName);
        }

        public void AddMissingColumn(string stageTable, string columnName)
        {
            _isCompatible = false;
            _missingColumns.Add($"{stageTable}.{columnName}");
        }

        internal void AddExtraTable(string tableName)
        {
            // extra tables in the source database. Not an error, but useful 
            // info during dev...
            _extraTables.Add(tableName);
        }

        public void AddExtraColumn(string stageTable, string columnName)
        {
            // extra columns in the source database. Not an error, but useful 
            // info during dev...
            _extraColumns.Add($"{stageTable}.{columnName}");
        }

        public void BadDataType(string stageTable, string columnName)
        {
            _isCompatible = false;
            _badDataTypes.Add($"{stageTable}.{columnName}");
        }

        public IReadOnlyList<string> MissingTables
        {
            get
            {
                _missingTables.Sort();
                return _missingTables;
            }
        }

        public IReadOnlyList<string> MissingColumns
        {
            get
            {
                _missingColumns.Sort();
                return _missingColumns;
            }
        }

        public IReadOnlyList<string> ExtraTables
        {
            get
            {
                _extraTables.Sort();
                return _extraTables;
            }
        }

        public IReadOnlyList<string> ExtraColumns
        {
            get
            {
                _extraColumns.Sort();
                return _extraColumns;
            }
        }

        public IReadOnlyList<string> BadDataTypes
        {
            get
            {
                _badDataTypes.Sort();
                return _badDataTypes;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var t in MissingTables)
            {
                sb.AppendFormat(Strings.MISSING_TABLE, t);
                sb.AppendLine();
            }

            foreach (var c in MissingColumns)
            {
                sb.AppendFormat(Strings.MISSING_COLUMN, c);
                sb.AppendLine();
            }

            foreach (var d in BadDataTypes)
            {
                sb.AppendFormat(Strings.BAD_DATA_TYPE, d);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
