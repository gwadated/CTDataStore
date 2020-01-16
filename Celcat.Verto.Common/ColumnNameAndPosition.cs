namespace Celcat.Verto.Common
{
    using System;

    public class ColumnNameAndPosition
    {
        public string ColumnName { get; set; }

        public int Position { get; set; }

        public static bool Identical(ColumnNameAndPosition c1, ColumnNameAndPosition c2)
        {
            return c1.ColumnName.Equals(c2.ColumnName, StringComparison.OrdinalIgnoreCase) &&
                   c1.Position == c2.Position;
        }
    }
}
