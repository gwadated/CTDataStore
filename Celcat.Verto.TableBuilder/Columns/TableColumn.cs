namespace Celcat.Verto.TableBuilder.Columns
{
    using System;
    using System.Data;
    using Celcat.Verto.Common;

    public class TableColumn
    {
        private readonly string _name;
        private readonly SqlDbType _sqlDbType;
        private readonly int _length;
        private readonly ColumnNullable _nullable;
        private readonly bool _identity;

        public string Name => _name;

        public SqlDbType SqlDbType => _sqlDbType;

        public int Length => _length;

        public ColumnNullable Nullable => _nullable;

        public bool Identity => _identity;

        public TableColumn()
        {
            _nullable = ColumnNullable.True;
        }

        public TableColumn(
            string name, SqlDbType sqlDbType, int length = 0, ColumnNullable nullable = ColumnNullable.True, bool identity = false)
        {
            _name = name;
            _sqlDbType = sqlDbType;
            _length = length;
            _nullable = nullable;
            _identity = identity;
        }

        public string ColumnDefinitionString
        {
            get
            {
                var nullableStr = _nullable == ColumnNullable.True ? "NULL" : "NOT NULL";
                var dataTypeStr = SqlDbType.ToString().ToLower();
                var lenStr = _length == int.MaxValue ? "max" : _length.ToString();
                var identityStr = _identity ? " IDENTITY (1, 1)" : string.Empty;

                return _length > 0

                   ? $"{DatabaseUtils.EscapeDbObject(Name)} {DatabaseUtils.EscapeDbObject(dataTypeStr)}({lenStr}) {nullableStr}{identityStr}"
                   : $"{DatabaseUtils.EscapeDbObject(Name)} {DatabaseUtils.EscapeDbObject(dataTypeStr)} {nullableStr}{identityStr}";
            }
        }
    }
}
