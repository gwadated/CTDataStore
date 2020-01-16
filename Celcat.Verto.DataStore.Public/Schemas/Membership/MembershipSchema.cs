namespace Celcat.Verto.DataStore.Public.Schemas.Membership
{
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    internal class MembershipSchema : PublicSchemaBase<PublicMembershipTable>
    {
        public const string MembershipSchemaName = "MEMBERSHIP";

        public MembershipSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(MembershipSchemaName, connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }
    }
}
