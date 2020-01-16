namespace Celcat.Verto.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ItemBatcher<T>
    {
        private readonly IEnumerable<T> _data;
        private readonly int _maxBatchSize;
        private readonly int _count;
        private int _currentIndex;

        public ItemBatcher(IEnumerable<T> sourceData, int maxBatchSize)
        {
            _data = sourceData;
            _maxBatchSize = maxBatchSize;
            _currentIndex = 0;
            _count = _data.Count();
        }

        public IEnumerable<T> GetBatch()
        {
            int numToTake = (_currentIndex + _maxBatchSize) > _count
               ? _count - _currentIndex
               : _maxBatchSize;

            var result = (numToTake == 0)
               ? null
               : _data.Skip(_currentIndex).Take(numToTake);

            _currentIndex += numToTake;

            return result;
        }

        public string GetAsCsv(IEnumerable<T> batch)
        {
            var sb = new StringBuilder();

            foreach (var item in batch)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(item);
            }

            return sb.ToString();
        }
    }
}
