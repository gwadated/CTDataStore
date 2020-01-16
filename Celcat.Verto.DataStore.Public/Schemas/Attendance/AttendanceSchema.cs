namespace Celcat.Verto.DataStore.Public.Schemas.Attendance
{
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    public class AttendanceSchema : PublicSchemaBase<PublicAttendanceTable>
    {
        public const string AttendanceSchemaName = "ATTENDANCE";

        public AttendanceSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(AttendanceSchemaName, connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }
    }
}
