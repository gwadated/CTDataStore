namespace Celcat.Verto.DataStore.Common.Schemas
{
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    public abstract class SchemaBase
    {
        protected string ConnectionString { get; }
        
        protected int TimeoutSecs { get; }
        
        protected int MaxDegreeOfParallelism { get; }
        
        protected Pipelines PipelineOptions { get; }

        protected abstract string SchemaName { get; }

        protected SchemaBase(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            ConnectionString = connectionString;
            TimeoutSecs = timeoutSecs;
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
            PipelineOptions = pipelineOptions;
        }

        protected void EnsureSchemaCreated()
        {
            if (!DatabaseUtils.SchemaExists(ConnectionString, TimeoutSecs, SchemaName))
            {
                DatabaseUtils.CreateSchema(ConnectionString, TimeoutSecs, SchemaName);
            }
        }

        protected string GetQualifiedTableName(string tableName)
        {
            return DatabaseUtils.GetQualifiedTableName(SchemaName, tableName);
        }

        protected void DropTablesInSchema()
        {
            DatabaseUtils.DropTablesInSchema(ConnectionString, TimeoutSecs, SchemaName);
        }
    }
}
