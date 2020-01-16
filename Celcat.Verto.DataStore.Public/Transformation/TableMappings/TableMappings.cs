namespace Celcat.Verto.DataStore.Public.Transformation.TableMappings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.DataStore.Public.Staging;
    using Celcat.Verto.TableBuilder;

    public class TableMappings : IEnumerable<TableMapping>
    {
        private readonly List<TableMapping> _mappings;
        private readonly PublicTablesBuilder<PublicResourceTable> _resTables;
        private readonly PublicTablesBuilder<PublicAttendanceTable> _attendanceTables;
        private readonly PublicTablesBuilder<PublicBookingTable> _bookingsTables;
        private readonly PublicTablesBuilder<PublicEventTable> _eventsTables;
        private readonly PublicTablesBuilder<PublicExamTable> _examTables;
        private readonly PublicTablesBuilder<PublicMembershipTable> _membershipTables;
        private readonly PublicTablesBuilder<PublicMiscTable> _miscTables;

        public TableMappings()
        {
            _mappings = new List<TableMapping>();
            _resTables = new PublicTablesBuilder<PublicResourceTable>();
            _attendanceTables = new PublicTablesBuilder<PublicAttendanceTable>();
            _bookingsTables = new PublicTablesBuilder<PublicBookingTable>();
            _eventsTables = new PublicTablesBuilder<PublicEventTable>();
            _examTables = new PublicTablesBuilder<PublicExamTable>();
            _membershipTables = new PublicTablesBuilder<PublicMembershipTable>();
            _miscTables = new PublicTablesBuilder<PublicMiscTable>();

            GenerateMappings();
        }

        private void GenerateMappings()
        {
            var b = PublicStagingTablesBuilder.Get();

            foreach (var table in b.GetTables())
            {
                Debug.Assert(table.Name.StartsWith("CT_"));

                var publicTableName = GetPublicTableName(table.Name);

                var publicTable = Find(publicTableName);
                _mappings.Add(new TableMapping { PublicStagingTable = table, PublicTable = publicTable });
            }
        }

        private string GetPublicTableName(string stagingTableName)
        {
            var publicTableName = stagingTableName.StartsWith("CT_ES") || stagingTableName.StartsWith("CT_AT")
               ? stagingTableName.Substring(6)
               : stagingTableName.Substring(3);

            // bespoke mappings...
            if (publicTableName.Equals("SPAN"))
            {
                publicTableName = "WEEK_SPAN";
            }

            if (publicTableName.Equals("USER"))
            {
                publicTableName = "TIMETABLE_USER";
            }

            if (publicTableName.Equals("CONFIG"))
            {
                publicTableName = "TIMETABLE_CONFIG";
            }

            return publicTableName;
        }

        public Table Find(string publicTableName)
        {
            var result = _resTables.GetTable(publicTableName);

            if (result == null)
            {
                result = _attendanceTables.GetTable(publicTableName);
            }

            if (result == null)
            {
                result = _bookingsTables.GetTable(publicTableName);
            }

            if (result == null)
            {
                result = _eventsTables.GetTable(publicTableName);
            }

            if (result == null)
            {
                result = _examTables.GetTable(publicTableName);
            }

            if (result == null)
            {
                result = _membershipTables.GetTable(publicTableName);
            }

            if (result == null)
            {
                result = _miscTables.GetTable(publicTableName);
            }

            if (result == null)
            {
                throw new ApplicationException($"Could not find public table: {publicTableName}");
            }

            return result;
        }

        public IEnumerator<TableMapping> GetEnumerator()
        {
            return _mappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_mappings).GetEnumerator();
        }
    }
}
