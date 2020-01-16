namespace Celcat.Verto.Common
{
    using System;
    using System.Collections;
    using System.Text;

    /// <summary>
    ///  unused 'quacking' dictionary. Formerly used to represent rows in a data table
    /// </summary>
    /// 
    public class FlexiDictionary : IDictionary
    {
        private readonly IDictionary _items;

        public FlexiDictionary()
        {
            _items = new Hashtable(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines if dictionary contains key
        /// </summary>
        /// <param name="key">
        /// Key value
        /// </param>
        /// <returns>
        /// True on success
        /// </returns>
        public bool Contains(object key)
        {
            return _items.Contains(key);
        }

        /// <summary>
        /// Adds an object to the dictionary
        /// </summary>
        /// <param name="key">
        /// Key
        /// </param>
        /// <param name="value">
        /// Value
        /// </param>
        public void Add(object key, object value)
        {
            _items.Add(key, value);
        }

        /// <summary>
        /// Clear the dictionary
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Returns an enumerator
        /// </summary>
        /// <returns>
        /// Enumerator
        /// </returns>
        public IDictionaryEnumerator GetEnumerator()
        {
            return new Hashtable(_items).GetEnumerator();
        }

        /// <summary>
        /// Removes an object from the dictionary via its key
        /// </summary>
        /// <param name="key">
        /// Key of the object to remove
        /// </param>
        public void Remove(object key)
        {
            _items.Remove(key);
        }

        /// <summary>
        /// Gets the object from a given key
        /// </summary>
        /// <param name="key">
        /// Key of object to get
        /// </param>
        /// <returns>
        /// The object
        /// </returns>
        public object this[object key]
        {
            get => _items[key];
            set => _items[key] = value == DBNull.Value ? null : value;
        }

        /// <summary>
        /// Gets a collection of the keys
        /// </summary>
        public ICollection Keys => _items.Keys;

        /// <summary>
        /// Gets a collection of the objects
        /// </summary>
        public ICollection Values => _items.Values;

        /// <summary>
        /// Determines if the dictionary is read-only
        /// </summary>
        public bool IsReadOnly => _items.IsReadOnly;

        /// <summary>
        /// Determines if the dictionary is of a fixed size
        /// </summary>
        public bool IsFixedSize => _items.IsFixedSize;

        /// <summary>
        /// Returns an enumerator
        /// </summary>
        /// <returns>
        /// Enumerator
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Copies objects from the dictionary to the array
        /// </summary>
        /// <param name="array">
        /// The destination array
        /// </param>
        /// <param name="index">
        /// The destination index at which to start copying
        /// </param>
        public void CopyTo(Array array, int index)
        {
            _items.CopyTo(array, index);
        }

        /// <summary>
        /// The number of items in the dictionary
        /// </summary>
        public int Count => _items.Count;

        public object SyncRoot => _items.SyncRoot;

        public bool IsSynchronized => _items.IsSynchronized;

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (DictionaryEntry item in _items)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(item.Key);
                sb.Append(" = ");

                if (item.Value is string)
                {
                    sb.Append("\"");
                    sb.Append(item.Value);
                    sb.Append("\"");
                }
                else
                {
                    sb.Append(item.Value);
                }
            }

            return sb.ToString();
        }
    }
}
