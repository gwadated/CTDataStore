namespace Celcat.Verto.TableBuilder
{
    using Celcat.Verto.Common;

    public class TableKeyPart
    {
        private readonly string _columnName;
        private readonly ColumnOrder _columnOrder;

        public string ColumnName => _columnName;

        public ColumnOrder ColumnOrder => _columnOrder;

        public TableKeyPart(string columnName, ColumnOrder columnOrder = ColumnOrder.Ascending)
        {
            _columnName = columnName;
            _columnOrder = columnOrder;
        }
    }
}
