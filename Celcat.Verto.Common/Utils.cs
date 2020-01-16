namespace Celcat.Verto.Common
{
    using System;
    using System.Text;

    public static class Utils
    {
        public static DayOfWeek ConvertFromCt7DayOfWeek(int value)
        {
            switch (value)
            {
                case 0: return DayOfWeek.Monday;
                case 1: return DayOfWeek.Tuesday;
                case 2: return DayOfWeek.Wednesday;
                case 3: return DayOfWeek.Thursday;
                case 4: return DayOfWeek.Friday;
                case 5: return DayOfWeek.Saturday;
                case 6: return DayOfWeek.Sunday;

                default:
                    throw new ApplicationException("Unknown day of week");
            }
        }

        public static string ColumnOrderToSqlString(ColumnOrder order)
        {
            return order == ColumnOrder.Ascending
               ? "ASC"
               : "DESC";
        }

        public static string GetMachineName()
        {
            var sb = new StringBuilder();

            var netBiosName = Environment.MachineName;
            var hostName = System.Net.Dns.GetHostName();

            if (!string.IsNullOrEmpty(netBiosName))
            {
                sb.Append(netBiosName);
            }

            if (!string.IsNullOrEmpty(hostName) && !string.Equals(netBiosName, hostName))
            {
                var inBrackets = sb.Length > 0;
                if (inBrackets)
                {
                    sb.Append(" (");
                }

                sb.Append(netBiosName);

                if (inBrackets)
                {
                    sb.Append(")");
                }
            }

            if (sb.Length == 0)
            {
                sb.Append("Unknown");
            }

            return sb.ToString();
        }

        public static string ToCsv(params string[] strings)
        {
            var sb = new StringBuilder();

            foreach (var s in strings)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(s);
            }

            return sb.ToString();
        }
    }
}
