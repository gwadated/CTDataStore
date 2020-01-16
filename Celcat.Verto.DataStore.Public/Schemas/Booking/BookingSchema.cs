namespace Celcat.Verto.DataStore.Public.Schemas.Booking
{
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    internal class BookingSchema : PublicSchemaBase<PublicBookingTable>
    {
        public const string BookingSchemaName = "BOOKING";

        public BookingSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(BookingSchemaName, connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }
    }
}
