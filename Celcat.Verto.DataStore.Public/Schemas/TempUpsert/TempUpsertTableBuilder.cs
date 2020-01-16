namespace Celcat.Verto.DataStore.Public.Schemas.TempUpsert
{
    using Celcat.Verto.TableBuilder;

    internal class TempUpsertTableBuilder : Builder
    {
        public TempUpsertTableBuilder(string tableName, PublicTable publicTable)
        {
            AddTable(new Table(tableName, publicTable));
        }
    }
}
