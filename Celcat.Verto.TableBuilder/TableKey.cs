namespace Celcat.Verto.TableBuilder
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public abstract class TableKey : IEnumerable<TableKeyPart>
    {
        protected List<TableKeyPart> Key { get; }

        protected TableKey(params TableKeyPart[] keyParts)
        {
            Key = new List<TableKeyPart>();

            foreach (var kp in keyParts)
            {
                AddKeyPart(kp);
            }
        }

        public TableKeyPart this[int index] => Key[index];

        public TableKey AddKeyPart(TableKeyPart part)
        {
            Key.Add(part);
            return this;
        }

        public TableKey AddKeyPart(string colName)
        {
            Key.Add(new TableKeyPart(colName));
            return this;
        }

        public int KeyPartsCount => Key.Count;

        protected TableKey()
        {
            Key = new List<TableKeyPart>();
        }

        public bool Contains(string columnName)
        {
            return Key.Exists(x => x.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerator<TableKeyPart> GetEnumerator()
        {
            return Key.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Key).GetEnumerator();
        }

        public string ColumnsAsCsv()
        {
            var sb = new StringBuilder();

            int keyPartCount = KeyPartsCount;

            for (int n = 0; n < keyPartCount; ++n)
            {
                var keyPart = Key[n];

                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(keyPart.ColumnName);
            }

            return sb.ToString();
        }
    }
}
