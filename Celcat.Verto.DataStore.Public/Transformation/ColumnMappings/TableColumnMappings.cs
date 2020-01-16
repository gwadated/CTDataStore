namespace Celcat.Verto.DataStore.Public.Transformation.ColumnMappings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Consolidation;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.TableBuilder.Columns;

    public class TableColumnMappings : IEnumerable<ColumnMappingBase>
    {
        private readonly List<ColumnMappingBase> _mappings;

        public EventExpansionDefinition EventExpansion { get; set; }

        public bool EventExpansionRequired => EventExpansion != null;

        public bool SpanExpansionRequired { get; set; }

        public TableColumnMappings()
        {
            _mappings = new List<ColumnMappingBase>();
        }

        public void AddSimpleMapping(string publicCol, string stagingCol = null)
        {
            _mappings.Add(new ColumnMappingStandard(publicCol, stagingCol));
        }

        public ColumnMappingBase this[int index] => _mappings[index];

        public string GetPublicColNamesAsCsv()
        {
            var sb = new StringBuilder();

            for (int n = 0; n < _mappings.Count; ++n)
            {
                if (n > 0)
                {
                    sb.Append(",");
                }

                sb.Append(_mappings[n].PublicColumn);
            }

            if (EventExpansionRequired)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(EventExpansion.PublicEventInstanceColumn);
                sb.Append(",");
                sb.Append(EventExpansion.PublicWeekColumn);
                sb.Append(",");
                sb.Append(EventExpansion.PublicWeekOccurrenceColumn);
            }

            return sb.ToString();
        }

        public string FindCorrespondingPublicTableColumn(string stagingTableColumn)
        {
            foreach (var mapping in _mappings)
            {
                if (mapping is ColumnMappingStandard m && 
                    m.StagingColumn.Equals(stagingTableColumn, StringComparison.OrdinalIgnoreCase))
                {
                    return m.PublicColumn;
                }
            }

            return null;
        }

        public string FindCorrespondingStagingTableColumn(string publicTableColumn)
        {
            foreach (var mapping in _mappings)
            {
                if (mapping is ColumnMappingStandard m && 
                    m.PublicColumn.Equals(publicTableColumn, StringComparison.OrdinalIgnoreCase))
                {
                    return m.StagingColumn;
                }
            }

            return null;
        }

        public int EventExpansionColumnCount
        {
            get
            {
                if (EventExpansion != null)
                {
                    return EventExpansion.PublicColumnCount;
                }

                return 0;
            }
        }

        public int Count => _mappings.Count;

        public int CountIncludingEventExpansion => _mappings.Count + EventExpansionColumnCount;

        public IEnumerator<ColumnMappingBase> GetEnumerator()
        {
            return _mappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_mappings).GetEnumerator();
        }

        public void AddFederatedIdMapping(string publicColName)
        {
            var m = new ColumnMappingStandard(publicColName, EntityUtils.GetFederatedFieldName(publicColName));
            _mappings.Add(m);
        }

        public void AddEventInstanceMapping(string stagingEventIdColumn = "federated_event_id", string stagingEventWeekColumn = "week")
        {
            var m = new ColumnMappingLookup.ColumnMappingEventInstance(stagingEventIdColumn, stagingEventWeekColumn);
            _mappings.Add(m);
        }

        public void AddColumnMappingLookup(string publicIdColumn, string publicNameColumn, ConsolidationType cType, DataStoreConfiguration c)
        {
            var entityType = ConsolidationTypeUtils.ToEntity(cType);
            AddColumnMappingLookup(publicIdColumn, publicNameColumn, entityType, c);
        }

        public void AddExplicitColumnMappingLookup(string stagingIdColumn, string publicIdColumn, string publicNameColumn, ConsolidationType cType)
        {
            AddExplicitColumnMappingLookup(stagingIdColumn, publicIdColumn, publicNameColumn, ConsolidationTypeUtils.ToEntity(cType));
        }
        
        public void AddColumnMappingResourceId(
            string stagingFederatedIdColumn, string stagingConsolidatedIdColumn, string stagingResourceTypeColumn, string publicIdColumn, string publicNameColumn)
        {
            var m = new ColumnMappingLookup.ColumnMappingResourceLookup(
                stagingFederatedIdColumn, stagingConsolidatedIdColumn, stagingResourceTypeColumn, publicIdColumn, publicNameColumn);

            _mappings.Add(m);
        }

        public void AddColumnMappingLookup(string publicIdColumn, string publicNameColumn, Entity eType, DataStoreConfiguration c)
        {
            var stagingColName = GetFederatedOrConsolidatedIdColName(c, eType, publicIdColumn);
            var m = new ColumnMappingLookup(stagingColName, publicIdColumn, publicNameColumn, eType);
            _mappings.Add(m);
        }

        public void AddExplicitColumnMappingLookup(string stagingIdColumn, string publicIdColumn, string publicNameColumn, Entity eType)
        {
            var m = new ColumnMappingLookup(stagingIdColumn, publicIdColumn, publicNameColumn, eType);
            _mappings.Add(m);
        }

        public void AddResourceIdColumnMapping(string publicResIdColName, string publicResTypeColName)
        {
            var m = new ColumnMappingResourceId(
                publicResIdColName, 
                publicResTypeColName,
                EntityUtils.GetFederatedFieldName(publicResIdColName),
                EntityUtils.GetConsolidatedFieldName(publicResIdColName));

            _mappings.Add(m);
        }

        public void AddConsolidatedOrFederatedIdMapping(DataStoreConfiguration c, Entity e, string publicColName)
        {
            var fldName = c.Consolidation.Get(e).None
               ? EntityUtils.GetFederatedFieldName(publicColName)
               : EntityUtils.GetConsolidatedFieldName(publicColName);

            var m = new ColumnMappingStandard(publicColName, fldName);
            _mappings.Add(m);
        }

        public void AddBooleanMapping(string publicCol, string stagingCol = null)
        {
            _mappings.Add(new ColumnMappingLookup.ColumnMappingBoolean(publicCol, stagingCol));
        }

        public void AddAuditMapping(DataStoreConfiguration c)
        {
            AddSimpleMapping("date_change");
            AddConsolidatedOrFederatedIdMapping(c, Entity.User, "user_id_change");
            AddColumnMappingLookup("user_id_change", "user_name_change", ConsolidationType.User, c);
        }

        public void AddOriginMapping()
        {
            AddFederatedIdMapping("origin_id");
            AddSimpleMapping("original_id");
        }

        public void AddEquipIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Equip, "equip_id");
            AddColumnMappingLookup("equip_id", "equip_unique_name", ConsolidationType.Equip, c);
            AddColumnMappingLookup("equip_id", "equip_name", ConsolidationType.Equip, c);
        }

        public void AddGroupIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Group, "group_id");
            AddColumnMappingLookup("group_id", "group_unique_name", ConsolidationType.Group, c);
            AddColumnMappingLookup("group_id", "group_name", ConsolidationType.Group, c);
        }

        public void AddSubGroupIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Group, "subgroup_id");
            AddColumnMappingLookup("subgroup_id", "subgroup_unique_name", ConsolidationType.Group, c);
            AddColumnMappingLookup("subgroup_id", "subgroup_name", ConsolidationType.Group, c);
        }

        public void AddModuleIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Module, "module_id");
            AddColumnMappingLookup("module_id", "module_unique_name", ConsolidationType.Module, c);
            AddColumnMappingLookup("module_id", "module_name", ConsolidationType.Module, c);
        }

        private string GetFederatedOrConsolidatedIdColName(DataStoreConfiguration c, Entity e, string idColName)
        {
            return c.Consolidation.Get(e).None
               ? EntityUtils.GetFederatedFieldName(idColName)
               : EntityUtils.GetConsolidatedFieldName(idColName);
        }

        public void AddRoomIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Room, "room_id");
            AddColumnMappingLookup("room_id", "room_unique_name", ConsolidationType.Room, c);
            AddColumnMappingLookup("room_id", "room_name", ConsolidationType.Room, c);
        }

        public void AddFixtureIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Fixture, "fixture_id");
            AddColumnMappingLookup("fixture_id", "fixture_name", ConsolidationType.Fixture, c);
        }

        public void AddStaffCatIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.StaffCat, "staff_cat_id");
            AddColumnMappingLookup("staff_cat_id", "staff_cat_name", ConsolidationType.StaffCat, c);
        }

        public void AddRoomLayoutIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Layout, "room_layout_id");
            AddColumnMappingLookup("room_layout_id", "room_layout_name", ConsolidationType.Layout, c);
        }

        public void AddDeptIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Dept, "dept_id");
            AddColumnMappingLookup("dept_id", "dept_name", ConsolidationType.Dept, c);
        }

        public void AddSiteIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Site, "site_id");
            AddColumnMappingLookup("site_id", "site_name", ConsolidationType.Site, c);
        }

        public void AddSupervisorIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Supervisor, "supervisor_id");
            AddColumnMappingLookup("supervisor_id", "supervisor_name", ConsolidationType.Supervisor, c);
        }

        public void AddCourseIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Course, "course_id");
            AddColumnMappingLookup("course_id", "course_name", ConsolidationType.Course, c);
        }

        public void AddSpanIdAndNameMapping()
        {
            AddFederatedIdMapping("span_id");
            AddExplicitColumnMappingLookup("federated_span_id", "span_id", "span_name", Entity.Span);
        }

        public void AddEventCatIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.EventCat, "event_cat_id");
            AddColumnMappingLookup("event_cat_id", "event_cat_name", ConsolidationType.EventCat, c);
        }

        public void AddFacultyIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Faculty, "faculty_id");
            AddColumnMappingLookup("faculty_id", "faculty_name", ConsolidationType.Faculty, c);
        }

        public void AddUserIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.User, "user_id");
            AddColumnMappingLookup("user_id", "user_name", ConsolidationType.User, c);
        }

        public void AddStaffIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Staff, "staff_id");
            AddColumnMappingLookup("staff_id", "staff_unique_name", ConsolidationType.Staff, c);
            AddColumnMappingLookup("staff_id", "staff_name", ConsolidationType.Staff, c);
        }

        public void AddStaff1And2IdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Staff, "staff_id1");
            AddColumnMappingLookup("staff_id1", "staff_unique_name1", ConsolidationType.Staff, c);
            AddColumnMappingLookup("staff_id1", "staff_name1", ConsolidationType.Staff, c);

            AddConsolidatedOrFederatedIdMapping(c, Entity.Staff, "staff_id2");
            AddColumnMappingLookup("staff_id2", "staff_unique_name2", ConsolidationType.Staff, c);
            AddColumnMappingLookup("staff_id2", "staff_name2", ConsolidationType.Staff, c);
        }

        public void AddStudentIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Student, "student_id");
            AddColumnMappingLookup("student_id", "student_unique_name", ConsolidationType.Student, c);
            AddColumnMappingLookup("student_id", "student_name", ConsolidationType.Student, c);
        }

        public void AddTeamIdAndNameMapping(DataStoreConfiguration c)
        {
            AddConsolidatedOrFederatedIdMapping(c, Entity.Team, "team_id");
            AddColumnMappingLookup("team_id", "team_unique_name", ConsolidationType.Team, c);
            AddColumnMappingLookup("team_id", "team_name", ConsolidationType.Team, c);
        }

        public void AddExamIdAndNameMapping()
        {
            AddFederatedIdMapping("exam_id");
            AddExplicitColumnMappingLookup("federated_exam_id", "exam_id", "exam_name", Entity.EsExam);
            AddExplicitColumnMappingLookup("federated_exam_id", "exam_id", "exam_unique_name", Entity.EsExam);
        }

        public void AddMarkIdAndNameMapping()
        {
            AddFederatedIdMapping("mark_id");
            AddExplicitColumnMappingLookup("federated_mark_id", "mark_id", "mark_name", Entity.AtMark);
        }

        public void AddResourceIdAndNameMapping()
        {
            AddSimpleMapping("resource_type");
            AddResourceIdColumnMapping("resource_id", "resource_type");
            AddColumnMappingResourceId("federated_resource_id", "consolidated_resource_id", "resource_type", "resource_id", "resource_unique_name");
            AddColumnMappingResourceId("federated_resource_id", "consolidated_resource_id", "resource_type", "resource_id", "resource_name");
        }

        public void AddDescriptionMapping()
        {
            AddSimpleMapping("description");
        }

        public void AddMarkDefinition()
        {
            AddSimpleMapping("definition");

            var m = new ColumnMappingLookup.ColumnMappingMarkDefinition("definition_str", "definition");
            _mappings.Add(m);
        }

        public void AddRemainingSimpleMappings(IReadOnlyList<TableColumn> columns)
        {
            foreach (var c in columns)
            {
                if (!_mappings.Exists(x => x.PublicColumn.Equals(c.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (EventExpansion == null || !EventExpansion.PublicColumnSpecified(c.Name))
                    {
                        AddSimpleMapping(c.Name);
                    }
                }
            }
        }

        public void AddEventStartEndTimeMapping(string colName)
        {
            var m = new ColumnMappingEventStartEndTime(colName);
            _mappings.Add(m);
        }

        public void AddSchedulableMapping()
        {
            AddBooleanMapping("schedulable");
        }

        public void AddSpecialNeedsMapping()
        {
            AddBooleanMapping("deaf_loop");
            AddBooleanMapping("wheelchair_access");
        }
    }
}
