namespace Celcat.Verto.DataStore.Common.Consolidation
{
    using System.Diagnostics;
    using Celcat.Verto.DataStore.Common.Entities;

    public static class ConsolidationTypeUtils
    {
        private const string ConsolidationColPrefix = "consolidated_";

        public static string GetIdFldName(ConsolidationType ctype)
        {
            var e = ToEntity(ctype);

            if (e != Entity.Unknown)
            {
                EntityUtils.GetIdFldName(e);
            }

            return null;
        }

        public static ConsolidationType GetConsolidationTypeFromIdFldName(string colName)
        {
            switch (colName)
            {
                case "course_id":
                    return ConsolidationType.Course;
                case "dept_id":
                    return ConsolidationType.Dept;
                case "equip_id":
                    return ConsolidationType.Equip;
                case "event_cat_id":
                    return ConsolidationType.EventCat;
                case "faculty_id":
                    return ConsolidationType.Faculty;
                case "fixture_id":
                    return ConsolidationType.Fixture;
                case "group_id":
                case "subgroup_id":
                    return ConsolidationType.Group;
                case "room_layout_id":
                    return ConsolidationType.Layout;
                case "module_id":
                    return ConsolidationType.Module;
                case "room_id":
                    return ConsolidationType.Room;
                case "site_id":
                case "site_id1":
                case "site_id2":
                    return ConsolidationType.Site;
                case "staff_id":
                case "staff_id1":
                case "staff_id2":
                    return ConsolidationType.Staff;
                case "staff_cat_id":
                case "pk_staff_cat_id":
                    return ConsolidationType.StaffCat;
                case "student_id":
                    return ConsolidationType.Student;
                case "supervisor_id":
                    return ConsolidationType.Supervisor;
                case "team_id":
                    return ConsolidationType.Team;
                case "user_id":
                case "user_id_change":
                    return ConsolidationType.User;

                default:
                    return ConsolidationType.None;
            }
        }

        public static Entity ToEntity(ConsolidationType ctype)
        {
            switch (ctype)
            {
                case ConsolidationType.Course:
                    return Entity.Course;
                case ConsolidationType.Dept:
                    return Entity.Dept;
                case ConsolidationType.Equip:
                    return Entity.Equip;
                case ConsolidationType.EventCat:
                    return Entity.EventCat;
                case ConsolidationType.Faculty:
                    return Entity.Faculty;
                case ConsolidationType.Fixture:
                    return Entity.Fixture;
                case ConsolidationType.Group:
                    return Entity.Group;
                case ConsolidationType.Layout:
                    return Entity.Layout;
                case ConsolidationType.Module:
                    return Entity.Module;
                case ConsolidationType.Room:
                    return Entity.Room;
                case ConsolidationType.Site:
                    return Entity.Site;
                case ConsolidationType.Staff:
                    return Entity.Staff;
                case ConsolidationType.StaffCat:
                    return Entity.StaffCat;
                case ConsolidationType.Student:
                    return Entity.Student;
                case ConsolidationType.Supervisor:
                    return Entity.Supervisor;
                case ConsolidationType.Team:
                    return Entity.Team;
                case ConsolidationType.User:
                    return Entity.User;
                default:
                    return Entity.Unknown;
            }
        }

        public static string GetConsolidatedFieldName(string colName)
        {
            return string.Concat(ConsolidationColPrefix, colName);
        }

        public static bool IsConsolidatedFieldName(string colName)
        {
            return colName.StartsWith(ConsolidationColPrefix);
        }

        public static string GetBaseFieldNameFromConsolidatedField(string colName)
        {
            Debug.Assert(IsConsolidatedFieldName(colName));
            return colName.Substring(ConsolidationColPrefix.Length);
        }
    }
}
