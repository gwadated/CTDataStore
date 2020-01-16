namespace Celcat.Verto.DataStore.Public.Schemas
{
    using System;
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Common.Schemas;
    using Celcat.Verto.DataStore.Public.Schemas.Attendance;
    using Celcat.Verto.DataStore.Public.Schemas.Booking;
    using Celcat.Verto.DataStore.Public.Schemas.Event;
    using Celcat.Verto.DataStore.Public.Schemas.Exam;
    using Celcat.Verto.DataStore.Public.Schemas.Misc;
    using Celcat.Verto.DataStore.Public.Schemas.Resources;
    using global::Common.Logging;

    public abstract class PublicSchemaBase<T> : SchemaBase 
        where T : PublicTable
    {
        public const int MaxWeeksInTimetable = 56;
        public const int MaxPeriodsPerDayInTimetable = 36;

        // doesn't matter that we have a separate _log for each type T
        // ReSharper disable once StaticMemberInGenericType
#pragma warning disable S2743 // Static fields should not be used in generic types
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning restore S2743 // Static fields should not be used in generic types

        private readonly string _schemaName;

        protected PublicSchemaBase(string schemaName, string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
            _schemaName = schemaName;
        }

        protected override string SchemaName => _schemaName;

        public void CreateTables()
        {
            _log.DebugFormat("Creating tables in schema: {0}", _schemaName);

            DropTablesInSchema();
            InternalCreateEmptyTables();
        }

        private void InternalCreateEmptyTables()
        {
            EnsureSchemaCreated();
            PublicTablesBuilder<T> b = new PublicTablesBuilder<T>();
            b.Execute(ConnectionString, TimeoutSecs);
        }

        public static string GetPublicSchemaName(Entity et)
        {
            switch (et)
            {
                case Entity.Course:
                case Entity.Module:
                case Entity.Group:
                case Entity.Staff:
                case Entity.Room:
                case Entity.Student:
                case Entity.Equip:
                case Entity.Team:
                case Entity.Faculty:
                case Entity.Dept:
                case Entity.Fixture:
                case Entity.Layout:
                case Entity.Site:
                case Entity.EventCat:
                case Entity.Supervisor:
                case Entity.StaffCat:
                    return ResourceSchema.ResourceSchemaName;

                case Entity.User:
                case Entity.Config:
                case Entity.WeekScheme:
                case Entity.Span:
                case Entity.Origin:
                    return MiscSchema.MiscSchemaName;

                case Entity.AtActivity:
                case Entity.AtAttend:
                case Entity.AtAttendTime:
                case Entity.AtException:
                case Entity.AtMark:
                case Entity.AtNotification:
                case Entity.AtStudentException:
                    return AttendanceSchema.AttendanceSchemaName;

                case Entity.Booking:
                    return BookingSchema.BookingSchemaName;

                case Entity.EsExam:
                case Entity.EsSession:
                case Entity.EsSlot:
                    return ExamSchema.ExamSchemaName;

                case Entity.Event:
                    return EventSchema.EventSchemaName;

                default:
                    throw new ArgumentOutOfRangeException(nameof(et), "Could not recognise entity");
            }
        }
    }
}
