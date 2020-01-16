namespace Celcat.Verto.DataStore.Public.Schemas.Misc
{
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    internal class MiscSchema : PublicSchemaBase<PublicMiscTable>
    {
        public const string MiscSchemaName = "MISC";

        public MiscSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(MiscSchemaName, connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }
    }
}
