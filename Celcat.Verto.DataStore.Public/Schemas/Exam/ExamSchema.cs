namespace Celcat.Verto.DataStore.Public.Schemas.Exam
{
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    internal class ExamSchema : PublicSchemaBase<PublicExamTable>
    {
        public const string ExamSchemaName = "EXAM";

        public ExamSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(ExamSchemaName, connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }
    }
}
