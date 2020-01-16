namespace Celcat.Verto.DataStore.Admin.Control.Mutex
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Reflection;
    using Celcat.Verto.Common;
    using global::Common.Logging;
    
    /// <summary>
    /// ControlMutex is a sempaphore stored in CONTROL.CONFIG.mutex_value
    /// Its use is designed to prevent multiple instances of the Verto process
    /// operating on a staging database concurrently.
    /// 
    /// At the start of the Verto ETL operation it grabs the mutex to ensure
    /// that it has the exclusive right to run. At 30 second intervals the
    /// process 'touches' the mutex, updating the time stamp and keeping it
    /// 'alive'
    /// 
    /// At the end of the Verto operation it releases the mutex.
    /// 
    /// If the operation is disrupted and the mutex not released, it is
    /// automatically released after 2 minutes
    /// </summary>
    internal static class ControlMutex
    {
        private const int MutexTimeoutMins = 2;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private static string GetQualifiedConfigTableName()
        {
            return DatabaseUtils.GetQualifiedTableName(ControlSchema.ControlSchemaName, ControlSchema.ConfigTableName);
        }

        public static bool Touch(string connectionString, int timeoutSecs, Guid mutexValue)
        {
            var machineName = Utils.GetMachineName();
            _log.DebugFormat("Touching mutex, machine: {0}", machineName);

            var sb = new SqlBuilder();
            sb.AppendFormat(
               "update {0} set mutex_touched=GETDATE()",
               GetQualifiedConfigTableName());
            sb.AppendFormat("where mutex_value = @G and config_id=1");

            SqlParameter[] p = { new SqlParameter("@G", SqlDbType.UniqueIdentifier) { Value = mutexValue } };

            return DatabaseUtils.ExecuteSql(connectionString, sb.ToString(), timeoutSecs, p) == 1;
        }

        public static ControlMutexResult Grab(string connectionString, int timeoutSecs)
        {
            var machineName = Utils.GetMachineName();
            _log.DebugFormat("Grabbing mutex, machine: {0}", machineName);

            var sb = new SqlBuilder();
            sb.AppendFormat(
               "update {0} set mutex_value=@G, mutex_machine_name=@M, mutex_start=GETDATE(), mutex_touched=GETDATE()",
               GetQualifiedConfigTableName());
            sb.AppendFormat("where (mutex_value is null or DATEDIFF(minute, mutex_touched, GETDATE()) >= {0}) and ", MutexTimeoutMins);
            sb.Append("config_id=1");

            var p = new List<SqlParameter> { new SqlParameter("@M", machineName) };
            var guidParam = new SqlParameter("@G", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() };
            p.Add(guidParam);

            var rowsAffected = DatabaseUtils.ExecuteSql(connectionString, sb.ToString(), timeoutSecs, p.ToArray());

            var result = new ControlMutexResult
            {
                Status = GetCurrentStatus(connectionString, timeoutSecs),
                Success = rowsAffected == 1
            };

            return result;
        }

        public static bool Release(string connectionString, int timeoutSecs, Guid mutexValue)
        {
            var sb = new SqlBuilder();
            sb.AppendFormat(
               "update {0} set mutex_value=null, mutex_machine_name=null, mutex_start=null, mutex_touched=null",
               GetQualifiedConfigTableName());
            sb.AppendFormat("where mutex_value=@G and config_id=1");

            SqlParameter[] p = { new SqlParameter("@G", SqlDbType.UniqueIdentifier) {Value = mutexValue} };

            return DatabaseUtils.ExecuteSql(connectionString, sb.ToString(), timeoutSecs, p) == 1;
        }

        public static ControlMutexStatus GetCurrentStatus(string connectionString, int timeoutSecs)
        {
            ControlMutexStatus result = null;

            var sql =
                $"select mutex_value, mutex_machine_name, mutex_start, mutex_touched from {GetQualifiedConfigTableName()} where config_id=1";

            DatabaseUtils.GetSingleResult(connectionString, sql, timeoutSecs, r =>
            {
                result = new ControlMutexStatus
                {
                    MutexValue = (Guid)DatabaseUtils.SafeRead(r, "mutex_value", Guid.Empty),
                    MachineName = (string)r["mutex_machine_name"],
                    Start = (DateTime)DatabaseUtils.SafeRead(r, "mutex_start", DateTime.MinValue),
                    Touched = (DateTime)DatabaseUtils.SafeRead(r, "mutex_touched", DateTime.MinValue)
                };
            });

            return result;
        }
    }
}
