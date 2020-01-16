namespace Celcat.Verto.DataStore.Admin.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Control;
    using Celcat.Verto.DataStore.Admin.Models;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    internal static class SourceTimetableRegistration
    {
        public static void EnsureSourceTimetablesAreRegistered(
            string adminConnectionString, 
            int timeoutSecs,
            IReadOnlyList<SourceTimetableData> timetables, 
            int maxDegreeOfParallelism, 
            Pipelines pipelineOptions)
        {
            var cs = new ControlSchema(adminConnectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions);
            var registeredTimetables = cs.GetSourceTimetableRecords();

            foreach (var tt in timetables)
            {
                var found = registeredTimetables.FirstOrDefault(x => x.Identifier == tt.Identifier);
                if (found != null)
                {
                    if (!found.Name.Equals(tt.Name))
                    {
                        throw new ApplicationException($"Found timetable with guid = {tt.Identifier} but registered under a different timetable name");
                    }
                }
                else
                {
                    found = registeredTimetables.FirstOrDefault(x => x.Name == tt.Name);
                    if (found != null)
                    {
                        throw new ApplicationException($"Found timetable with name = {tt.Name} but registered under a different guid");
                    }
                }

                if (found == null)
                {
                    RegisterSourceTimetable(adminConnectionString, timeoutSecs, tt);
                }
            }
        }

        /// <summary>
        /// registers the specified timetable in the admin database
        /// </summary>
        /// <param name="adminConnectionString"></param>
        /// <param name="timeoutSecs"></param>
        /// <param name="tt"></param>
        /// <returns>
        /// The id of the newly-inserted row
        /// </returns>
        private static int RegisterSourceTimetable(string adminConnectionString, int timeoutSecs, SourceTimetableData tt)
        {
            var sb = new SqlBuilder();

            sb.AppendFormat(
                "insert into {0} (timetable_name, server_name, database_name, schema_version, guid)",
                DatabaseUtils.GetQualifiedTableName(ControlSchema.ControlSchemaName, ControlSchema.SrcTimetableName));

            sb.Append("values (@N, @S, @D, @V, @G); select @@IDENTITY");

            var p = new List<SqlParameter>
            {
                new SqlParameter("@N", tt.Name),
                new SqlParameter("@S", tt.SqlServerName),
                new SqlParameter("@D", tt.DatabaseName),
                new SqlParameter("@V", tt.SchemaVersion)
            };

            var guidParam = new SqlParameter("@G", SqlDbType.UniqueIdentifier) { Value = tt.Identifier };
            p.Add(guidParam);

            int result =
               Convert.ToInt32(DatabaseUtils.ExecuteScalar(adminConnectionString, sb.ToString(), timeoutSecs, p.ToArray()));

            return result;
        }
    }
}
