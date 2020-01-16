namespace Celcat.Verto.Common
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A simple representation of a data row
    /// </summary>
    public class SimpleTableRow
    {
        private readonly List<object> _columnValues;

        public SimpleTableRow()
        {
            _columnValues = new List<object>();
        }

        /// <summary>
        /// Add a value to the row (this is appended to the list of row values)
        /// so order is important!
        /// </summary>
        /// <param name="value">The value to add</param>
        public void Add(object value)
        {
            _columnValues.Add(value);
        }

        /// <summary>
        /// Get the row value at a specified column index
        /// </summary>
        /// <param name="column">
        /// The column index
        /// </param>
        /// <returns>Row value</returns>
        public object Get(int column)
        {
            return _columnValues[column];
        }

        /// <summary>
        /// Get the row value at a specified column index
        /// </summary>
        /// <param name="column">
        /// The column index
        /// </param>
        /// <returns>Row value</returns>
        public object this[int column] => Get(column);

        /// <summary>
        /// Get the number of columns in the row 
        /// </summary>
        public int ColumnCount => _columnValues.Count;

        /// <summary>
        /// Generates a string representation of the row
        /// </summary>
        /// <returns>
        /// The row as a string
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("{");

            for (int n = 0; n < _columnValues.Count; ++n)
            {
                if (n > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(_columnValues[n]);
            }

            sb.Append("}");

            return sb.ToString();
        }
    }
}
