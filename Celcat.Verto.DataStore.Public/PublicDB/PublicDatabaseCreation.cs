namespace Celcat.Verto.DataStore.Public.PublicDB
{
    using System;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Public.MetaData;
    using Celcat.Verto.DataStore.Public.Schemas.Attendance;
    using Celcat.Verto.DataStore.Public.Schemas.Booking;
    using Celcat.Verto.DataStore.Public.Schemas.Event;
    using Celcat.Verto.DataStore.Public.Schemas.Exam;
    using Celcat.Verto.DataStore.Public.Schemas.Membership;
    using Celcat.Verto.DataStore.Public.Schemas.Misc;
    using Celcat.Verto.DataStore.Public.Schemas.Resources;
    using Celcat.Verto.DataStore.Public.Staging;

    internal static class PublicDatabaseCreation
    {
        public static void Execute(Guid appKey, string connectionString, int timeout, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            // admin database doesn't yet exist...
            DatabaseUtils.CreateDatabase(connectionString, timeout);
            CreatePublicDatabaseObjects(appKey, connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
        }

        public static void CreatePublicDatabaseObjects(Guid appKey, string connectionString, int timeout, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            // metadata tables...
            CreateMetaDataTables(appKey, connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);

            // create empty staging tables in Public STAGING schema
            CreateStagingTables(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);

            // create public-facing tables...
            CreatePublicFacingTables(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
        }

        private static void CreatePublicFacingTables(string connectionString, int timeout, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            {
                // resources...
                var schema = new ResourceSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
                schema.CreateTables();
            }

            {
                // attendance...
                var schema = new AttendanceSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
                schema.CreateTables();
            }

            {
                // bookings...
                var schema = new BookingSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
                schema.CreateTables();
            }

            {
                // events...
                var schema = new EventSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
                schema.CreateTables();
            }

            {
                // exams...
                var schema = new ExamSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
                schema.CreateTables();
            }

            {
                // membership...
                var schema = new MembershipSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
                schema.CreateTables();
            }

            {
                // misc...
                var schema = new MiscSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
                schema.CreateTables();
            }
        }

        private static void CreateStagingTables(string connectionString, int timeout, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            PublicStagingSchema stage = new PublicStagingSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
            stage.CreateTables();
        }

        private static void CreateMetaDataTables(Guid appKey, string connectionString, int timeout, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            MetaDataSchema md = new MetaDataSchema(connectionString, timeout, maxDegreeOfParallelism, pipelineOptions);
            md.CreateTables(appKey);
        }
    }
}
