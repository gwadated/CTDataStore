namespace Celcat.Verto.Common
{
    using System.Data;
    using System.Linq;

    public class SimpleTableData : TableDataBase
    {
        public void AddFromReader(IDataReader reader)
        {
            var fillColumnNames = !ColumnNames.Any();

            var result = new SimpleTableRow();
            for (var n = 0; n < reader.FieldCount; ++n)
            {
                result.Add(reader.GetValue(n));

                if (fillColumnNames)
                {
                    AddColumn(reader.GetName(n));
                }
            }

            AddRow(result);
        }
    }
}
