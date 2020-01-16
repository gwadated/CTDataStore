namespace Celcat.Verto.Common
{
    using System;
    using System.Collections.Generic;

    public class PrimaryKeyInfo
    {
        public string TableName { get; set; }

        public List<ColumnNameAndPosition> Columns { get; set; }

        public static bool Identical(PrimaryKeyInfo pkInfo1, PrimaryKeyInfo pkInfo2)
        {
            if (!pkInfo1.TableName.Equals(pkInfo2.TableName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (pkInfo1.Columns.Count != pkInfo2.Columns.Count)
            {
                return false;
            }

            for (var n = 0; n < pkInfo1.Columns.Count; ++n)
            {
                if (!ColumnNameAndPosition.Identical(pkInfo1.Columns[n], pkInfo2.Columns[n]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
