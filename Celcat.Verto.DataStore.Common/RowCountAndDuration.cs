namespace Celcat.Verto.DataStore.Common
{
    using System;

    public class RowCountAndDuration
    {
        public long RowCount { get; set; }
    
        public TimeSpan Duration { get; set; }

        public RowCountAndDuration()
        {
            Duration = TimeSpan.Zero;
        }

        public static RowCountAndDuration operator +(RowCountAndDuration left, RowCountAndDuration right)
        {
            return new RowCountAndDuration
            {
                RowCount = left.RowCount + right.RowCount,
                Duration = left.Duration + right.Duration
            };
        }
    }
}
