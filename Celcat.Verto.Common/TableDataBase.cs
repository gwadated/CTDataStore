namespace Celcat.Verto.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base class for representing tabular data using small memory footprint
    /// </summary>
    /// 
    public abstract class TableDataBase : IEnumerable
    {
        private readonly List<string> _columnNames;
        private readonly List<SimpleTableRow> _rows;

        protected TableDataBase()
        {
            _columnNames = new List<string>();
            _rows = new List<SimpleTableRow>();
        }

        public int ColumnCount => _columnNames.Count;

        public void AddColumn(string columnName)
        {
            if (_columnNames.Contains(columnName, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Duplicate column names are not allowed");
            }

            _columnNames.Add(columnName);
        }

        public void AddRow(SimpleTableRow row)
        {
            if (!_columnNames.Any())
            {
                throw new ArgumentException("Columns are not yet specified");
            }

            var numCols = _columnNames.Count;
            var numRowCols = row.ColumnCount;
            if (numCols < numRowCols)
            {
                throw new ArgumentException("Row has too many columns");
            }

            while (numCols > numRowCols)
            {
                row.Add(null);
                ++numRowCols;
            }

            _rows.Add(row);
        }

        /// <summary>
        /// The column names
        /// </summary>
        public IEnumerable<string> ColumnNames => _columnNames;

        /// <summary>
        /// The row data
        /// </summary>
        public IEnumerable<SimpleTableRow> Rows => _rows;

        /// <summary>
        /// Enumerator
        /// </summary>
        /// <returns>
        /// Enumerator for the rows in the table
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _rows.GetEnumerator();
        }
    }
}
