namespace Celcat.Verto.Common.TableDiff
{
    public class TableDiffer : ResultSetDiffer
    {
        public TableDiffer(
            string connectionString, 
            int commandTimeoutSecs,
            string oldTable, 
            string newTable, 
            int primaryKeyColCount = 1)
           : base(
               connectionString, 
               commandTimeoutSecs,
               $"select * from {DatabaseUtils.EscapeDbObject(oldTable)}",
               $"select * from {DatabaseUtils.EscapeDbObject(newTable)}",
               primaryKeyColCount)
        {
        }
    }
}
