namespace Celcat.Verto.TableBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ColumnReferenceCheck
    {
        private readonly List<string> _primaryColumnNames;
        private readonly List<string> _localColumnNames;
        private readonly string _primaryTableName;

        public IReadOnlyList<string> PrimaryColumnNames => _primaryColumnNames;

        public IReadOnlyList<string> LocalColumnNames => _localColumnNames;
        
        public string PrimaryTableName => _primaryTableName;
        
        public int ColumnCount => _primaryColumnNames.Count;

        public ColumnReferenceCheck(string primaryTableName, params string[] columnNames)
        {
            _primaryTableName = primaryTableName;

            _primaryColumnNames = columnNames.ToList();
            _localColumnNames = columnNames.ToList();
        }

        public ColumnReferenceCheck(string primaryTableName, string[] primaryColumnNames, string[] localColumnNames)
        {
            _primaryTableName = primaryTableName;

            _primaryColumnNames = primaryColumnNames.ToList();
            _localColumnNames = localColumnNames.ToList();

            if (PrimaryColumnNames.Count != _localColumnNames.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public string LocalColumnNamesAsCsv()
        {
            var sb = new StringBuilder();

            foreach (var col in _localColumnNames)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(col);
            }

            return sb.ToString();
        }
    }
}
