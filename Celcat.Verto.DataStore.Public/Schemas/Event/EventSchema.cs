namespace Celcat.Verto.DataStore.Public.Schemas.Event
{
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    internal class EventSchema : PublicSchemaBase<PublicEventTable>
    {
        public const string EventSchemaName = "EVENT";

        public EventSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(EventSchemaName, connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }
    }
}
