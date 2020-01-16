namespace Celcat.Verto.DataStore.Public.Schemas.Resources
{
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    
    internal class ResourceSchema : PublicSchemaBase<PublicResourceTable>
    {
        public const string ResourceSchemaName = "RESOURCE";

        public ResourceSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(ResourceSchemaName, connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }
    }
}
