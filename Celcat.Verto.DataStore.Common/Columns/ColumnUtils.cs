namespace Celcat.Verto.DataStore.Common.Columns
{
    using System.Collections.Generic;
    using Celcat.Verto.TableBuilder.Columns;

    public static class ColumnUtils
    {
        private const int NumCustomFlds = 3;

        public static TableColumn[] CreateCustomColumns(int count = NumCustomFlds)
        {
            var result = new List<TableColumn>();

            for (var n = 0; n < count; ++n)
            {
                var colName = string.Concat("custom", (n + 1).ToString());
                result.Add(new StringColumn(colName, ColumnConstants.StrLenLookup));
            }

            return result.ToArray();
        }

        public static TableColumn[] CreateStdTelColumns()
        {
            return new TableColumn[]
            {
                new Ct7TelephoneColumn("office_tel"),
                new Ct7TelephoneColumn("home_tel"),
                new Ct7TelephoneColumn("mobile"),
                new Ct7TelephoneColumn("fax")
            };
        }

        public static TableColumn[] CreateSpecialNeedsColumns()
        {
            return new TableColumn[]
            {
                new Ct7BoolColumn("deaf_loop"),
                new Ct7BoolColumn("wheelchair_access")
            };
        }

        public static TableColumn[] CreateAddressColumns()
        {
            return new TableColumn[]
            {
                new StringColumn("address1", ColumnConstants.StrLenStd),
                new StringColumn("address2", ColumnConstants.StrLenStd),
                new StringColumn("address3", ColumnConstants.StrLenStd),
                new StringColumn("address4", ColumnConstants.StrLenStd),
                new StringColumn("postcode", ColumnConstants.StrLenStd)
            };
        }

        public static TableColumn[] CreateResourceTypeAndIdColumns(ColumnNullable nullable = ColumnNullable.True)
        {
            return new TableColumn[]
            {
                new IntColumn("resource_type", nullable),
                new BigIntColumn("resource_id", nullable)
            };
        }

        public static TableColumn[] CreateResourceTypeAndIdColumnsWithName(ColumnNullable nullable)
        {
            return new TableColumn[]
            {
                new IntColumn("resource_type", nullable),
                new BigIntColumn("resource_id", nullable),
                new Ct7UniqueNameColumn("resource_unique_name"),
                new Ct7NameColumn("resource_name")
            };
        }

        public static TableColumn[] CreateTargetColumns()
        {
            return new TableColumn[]
            {
                new IntColumn("weekly_target"),
                new IntColumn("total_target")
            };
        }

        public static TableColumn[] CreateStaff1And2Columns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("staff_id1"),
                new BigIntColumn("staff_id2")
            };
        }

        public static TableColumn[] CreateStaff1And2ColumnsWithNames()
        {
            return new TableColumn[]
            {
                new BigIntColumn("staff_id1"),
                new NullStringColumn("staff_unique_name1"),
                new NullStringColumn("staff_name1"),
                new BigIntColumn("staff_id2"),
                new NullStringColumn("staff_unique_name2"),
                new NullStringColumn("staff_name2"),
            };
        }

        public static TableColumn[] CreateDeptIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("dept_id"),
                new NullStringColumn("dept_name")
            };
        }

        public static TableColumn[] CreateStudentIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("student_id"),
                new NullStringColumn("student_unique_name"),
                new NullStringColumn("student_name")
            };
        }

        public static TableColumn[] CreateUserIdAndNameColumns(ColumnNullable nullable = ColumnNullable.True)
        {
            return new TableColumn[]
            {
                new BigIntColumn("user_id", nullable),
                new StringColumn("user_name", ColumnConstants.StrLenStd, nullable)
            };
        }

        public static TableColumn[] CreateSpanIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("span_id"),
                new NullStringColumn("span_name")
            };
        }

        public static TableColumn[] CreateEventCatIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("event_cat_id"),
                new NullStringColumn("event_cat_name")
            };
        }

        public static TableColumn[] CreateRoomIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("room_id"),
                new NullStringColumn("room_unique_name"),
                new NullStringColumn("room_name")
            };
        }

        public static TableColumn[] CreateStaffIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("staff_id"),
                new NullStringColumn("staff_unique_name"),
                new NullStringColumn("staff_name")
            };
        }

        public static TableColumn[] CreateSiteIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("site_id"),
                new NullStringColumn("site_name")
            };
        }

        public static TableColumn[] CreateFacultyIdAndNameColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("faculty_id"),
                new NullStringColumn("faculty_name")
            };
        }

        public static TableColumn[] CreateLookupColumns()
        {
            var result = new List<TableColumn>();

            const int numLookupFlds = 3;

            for (var n = 0; n < numLookupFlds; ++n)
            {
                string colName = string.Concat("lookup_id", (n + 1).ToString());
                result.Add(new StringColumn(colName, ColumnConstants.StrLenStd));
            }

            return result.ToArray();
        }

        public static TableColumn[] CreateAuditColumns()
        {
            return new TableColumn[]
            {
                new DateTimeColumn("date_change"),
                new BigIntColumn("user_id_change")
            };
        }

        public static TableColumn[] CreateAuditColumnsWithNames(ColumnNullable nullable = ColumnNullable.True)
        {
            return new TableColumn[]
            {
                new DateTimeColumn("date_change", nullable),
                new BigIntColumn("user_id_change", nullable),
                new StringColumn("user_name_change", ColumnConstants.StrLenStd, nullable)
            };
        }

        public static TableColumn[] CreateOriginColumns()
        {
            return new TableColumn[]
            {
                new BigIntColumn("origin_id"),
                new StringColumn("original_id", ColumnConstants.StrLenOriginalId)
            };
        }

        public static TableColumn CreateSchedulableColumn()
        {
            return new BitColumn("schedulable");
        }
    }
}
