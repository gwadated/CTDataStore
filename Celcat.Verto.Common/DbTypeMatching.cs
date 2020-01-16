namespace Celcat.Verto.Common
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public static class DbTypeMatching
    {
        private static readonly Dictionary<Type, DbType> _typeMap = new Dictionary<Type, DbType>()
        {
            {typeof(byte), DbType.Byte},
            {typeof(sbyte), DbType.SByte},
            {typeof(short), DbType.Int16},
            {typeof(ushort), DbType.UInt16},
            {typeof(int), DbType.Int32},
            {typeof(uint), DbType.UInt32},
            {typeof(long), DbType.Int64},
            {typeof(ulong), DbType.UInt64},
            {typeof(float), DbType.Single},
            {typeof(double), DbType.Double},
            {typeof(decimal), DbType.Decimal},
            {typeof(bool), DbType.Boolean},
            {typeof(string), DbType.String},
            {typeof(char), DbType.StringFixedLength},
            {typeof(Guid), DbType.Guid},
            {typeof(DateTime), DbType.DateTime},
            {typeof(DateTimeOffset), DbType.DateTimeOffset},
            {typeof(byte[]), DbType.Binary},
            {typeof(byte?), DbType.Byte},
            {typeof(sbyte?), DbType.SByte},
            {typeof(short?), DbType.Int16},
            {typeof(ushort?), DbType.UInt16},
            {typeof(int?), DbType.Int32},
            {typeof(uint?), DbType.UInt32},
            {typeof(long?), DbType.Int64},
            {typeof(ulong?), DbType.UInt64},
            {typeof(float?), DbType.Single},
            {typeof(double?), DbType.Double},
            {typeof(decimal?), DbType.Decimal},
            {typeof(bool?), DbType.Boolean},
            {typeof(char?), DbType.StringFixedLength},
            {typeof(Guid?), DbType.Guid},
            {typeof(DateTime?), DbType.DateTime},
            {typeof(DateTimeOffset?), DbType.DateTimeOffset}
        };

        public static DbType GetDbType(Type t)
        {
            return _typeMap[t];
        }

        public static bool MatchingDataTypes(string srcDataType, int srcMaxLen, SqlDbType stagingType, int stagingMaxLen)
        {
            switch (srcDataType)
            {
                case "int":
                    return SqlDbTypeIn(stagingType, SqlDbType.BigInt, SqlDbType.Int);

                case "nvarchar":
                    return SqlDbTypeIn(stagingType, SqlDbType.NVarChar) && srcMaxLen <= stagingMaxLen;

                case "varchar":
                    return SqlDbTypeIn(stagingType, SqlDbType.VarChar, SqlDbType.NVarChar) && srcMaxLen <= stagingMaxLen;

                case "nchar":
                    return SqlDbTypeIn(stagingType, SqlDbType.NVarChar, SqlDbType.NChar) &&
                       srcMaxLen <= stagingMaxLen;

                case "char":
                    return SqlDbTypeIn(stagingType, SqlDbType.Char, SqlDbType.NVarChar, SqlDbType.NChar, SqlDbType.VarChar) &&
                       srcMaxLen <= stagingMaxLen;

                case "float":
                    return SqlDbTypeIn(stagingType, SqlDbType.Float);

                case "datetime":
                    return SqlDbTypeIn(stagingType, SqlDbType.DateTime);

                case "uniqueidentifier":
                    return SqlDbTypeIn(stagingType, SqlDbType.UniqueIdentifier);

                default:
                    throw new ApplicationException(string.Format("Unrecognised column data type: {0}", srcDataType));
            }
        }

        public static Type GetClrType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return typeof(long?);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return typeof(bool?);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return typeof(DateTime?);

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(decimal?);

                case SqlDbType.Float:
                    return typeof(double?);

                case SqlDbType.Int:
                    return typeof(int?);

                case SqlDbType.Real:
                    return typeof(float?);

                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid?);

                case SqlDbType.SmallInt:
                    return typeof(short?);

                case SqlDbType.TinyInt:
                    return typeof(byte?);

                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return typeof(object);

                case SqlDbType.Structured:
                    return typeof(DataTable);

                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset?);

                default:
                    throw new ArgumentOutOfRangeException(nameof(sqlType));
            }
        }

        private static bool SqlDbTypeIn(SqlDbType t, params SqlDbType[] types)
        {
            return types.Contains(t);
        }
    }
}
