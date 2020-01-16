namespace Celcat.Verto.DataStore.Public.MetaData.Tables
{
    using Celcat.Verto.TableBuilder;

    internal class MetaDataTable : Table
    {
        protected MetaDataTable(string tableName)
           : base(tableName, MetaDataSchema.MetadataSchemaName)
        {
        }
    }
}
