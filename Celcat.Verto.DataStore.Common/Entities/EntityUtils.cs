namespace Celcat.Verto.DataStore.Common.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public static class EntityUtils
    {
        private const string FederationColPrefix = "federated_";
        private const string ConsolidationColPrefix = "consolidated_";

        private static string InsertUnderscores(string s)
        {
            var sb = new StringBuilder();

            foreach (var ch in s)
            {
                if (char.IsUpper(ch) && sb.Length > 0)
                {
                    sb.Append("_");
                }

                sb.Append(ch);
            }

            return sb.ToString();
        }

        public static string ToFederationTableName(Entity e)
        {
            if (e == Entity.Unknown)
            {
                throw new ApplicationException($"Unknown entity: {e}");
            }

            return InsertUnderscores(e.ToString()).ToUpper();
        }

        public static string ToConsolidationTableName(Entity e)
        {
            return string.Concat("CONSOLIDATION_", ToFederationTableName(e));
        }

        public static string ToConsolidationDetailTableName(Entity e)
        {
            return string.Concat("CONSOLIDATION_DETAIL_", ToFederationTableName(e));
        }

        public static string ToCtTableName(Entity e)
        {
            return string.Concat("CT_", ToFederationTableName(e));
        }

        public static string GetIdFldName(Entity et)
        {
            string name;

            switch (et)
            {
                case Entity.Course: name = "course_id"; break;
                case Entity.Module: name = "module_id"; break;
                case Entity.Group: name = "group_id"; break;
                case Entity.Staff: name = "staff_id"; break;
                case Entity.Room: name = "room_id"; break;
                case Entity.Student: name = "student_id"; break;
                case Entity.Equip: name = "equip_id"; break;
                case Entity.Team: name = "team_id"; break;
                case Entity.Faculty: name = "faculty_id"; break;
                case Entity.Dept: name = "dept_id"; break;
                case Entity.Fixture: name = "fixture_id"; break;
                case Entity.Layout: name = "room_layout_id"; break;
                case Entity.Site: name = "site_id"; break;
                case Entity.EventCat: name = "event_cat_id"; break;
                case Entity.Supervisor: name = "supervisor_id"; break;
                case Entity.StaffCat: name = "staff_cat_id"; break;
                case Entity.User: name = "user_id"; break;
                case Entity.AtActivity: name = "activity_id"; break;
                case Entity.AtAttend: name = "attend_id"; break;
                case Entity.AtAttendTime: name = "attend_time_id"; break;
                case Entity.AtException: name = "exception_id"; break;
                case Entity.AtMark: name = "mark_id"; break;
                case Entity.AtNotification: name = "message_id"; break;
                case Entity.AtStudentException: name = "student_exception_id"; break;
                case Entity.Booking: name = "booking_id"; break;
                case Entity.Config: name = "config_id"; break;
                case Entity.EsExam: name = "exam_id"; break;
                case Entity.EsSession: name = "session_id"; break;
                case Entity.EsSlot: name = "slot_id"; break;
                case Entity.Event: name = "event_id"; break;
                case Entity.Origin: name = "origin_id"; break;
                case Entity.Span: name = "span_id"; break;
                case Entity.WeekScheme: name = "week_scheme_id"; break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(et), "Could not recognise entity");
            }

            return name;
        }

        public static Entity GetEntityFromIdFldName(string colName)
        {
            switch (colName)
            {
                case "course_id":
                    return Entity.Course;
                case "module_id":
                    return Entity.Module;
                case "group_id":
                case "subgroup_id":
                    return Entity.Group;
                case "staff_id":
                case "staff_id1":
                case "staff_id2":
                    return Entity.Staff;
                case "room_id":
                    return Entity.Room;
                case "student_id":
                    return Entity.Student;
                case "equip_id":
                    return Entity.Equip;
                case "team_id":
                    return Entity.Team;
                case "faculty_id":
                    return Entity.Faculty;
                case "dept_id":
                    return Entity.Dept;
                case "fixture_id":
                    return Entity.Fixture;
                case "room_layout_id":
                    return Entity.Layout;
                case "site_id":
                case "site_id1":
                case "site_id2":
                    return Entity.Site;
                case "event_cat_id":
                    return Entity.EventCat;
                case "supervisor_id":
                    return Entity.Supervisor;
                case "staff_cat_id":
                case "pk_staff_cat_id":
                    return Entity.StaffCat;
                case "user_id":
                case "user_id_change":
                    return Entity.User;
                case "activity_id":
                    return Entity.AtActivity;
                case "attend_id":
                    return Entity.AtAttend;
                case "attend_time_id":
                    return Entity.AtAttendTime;
                case "exception_id":
                    return Entity.AtException;
                case "mark_id":
                    return Entity.AtMark;
                case "message_id":
                    return Entity.AtNotification;
                case "student_exception_id":
                    return Entity.AtStudentException;
                case "booking_id":
                    return Entity.Booking;
                case "config_id":
                    return Entity.Config;
                case "exam_id":
                    return Entity.EsExam;
                case "session_id":
                    return Entity.EsSession;
                case "slot_id":
                    return Entity.EsSlot;
                case "event_id":
                    return Entity.Event;
                case "origin_id":
                    return Entity.Origin;
                case "span_id":
                    return Entity.Span;
                case "week_scheme_id":
                    return Entity.WeekScheme;

                default:
                    return Entity.Unknown;
            }
        }
        
        public static bool CanParticipateInConsolidation(Entity e)
        {
            switch (e)
            {
                case Entity.Course:
                case Entity.Module:
                case Entity.Group:
                case Entity.Staff:
                case Entity.Room:
                case Entity.Student:
                case Entity.Equip:
                case Entity.Team:
                case Entity.Faculty:
                case Entity.Dept:
                case Entity.Fixture:
                case Entity.Layout:
                case Entity.Site:
                case Entity.EventCat:
                case Entity.Supervisor:
                case Entity.StaffCat:
                case Entity.User:
                    return true;

                default:
                    return false;
            }
        }

        public static IEnumerable<string> GetValidConsolidationColumns(Entity e)
        {
            switch (e)
            {
                case Entity.Course:
                case Entity.Module:
                case Entity.Group:
                case Entity.Room:
                case Entity.Equip:
                case Entity.Team:
                    return new[]
                    {
                      "name",
                      "unique_name",
                      "custom1",
                      "custom2",
                      "custom3",
                      "lookup_id1",
                      "lookup_id2",
                      "lookup_id3",
                      "original_id"
                    };

                case Entity.Staff:
                case Entity.Student:
                    return new[]
                    {
                      "name",
                      "unique_name",
                      "profile",
                      "email",
                      "custom1",
                      "custom2",
                      "custom3",
                      "lookup_id1",
                      "lookup_id2",
                      "lookup_id3",
                      "original_id"
                    };

                case Entity.Faculty:
                case Entity.Dept:
                case Entity.Fixture:
                case Entity.Layout:
                case Entity.Site:
                case Entity.EventCat:
                case Entity.Supervisor:
                case Entity.StaffCat:
                    return new[]
                    {
                      "name",
                      "lookup_id1",
                      "lookup_id2",
                      "lookup_id3",
                      "original_id"
                    };

                case Entity.User:
                    return new[]
                    {
                      "name",
                      "email",
                      "lookup_id1",
                      "lookup_id2",
                      "lookup_id3"
                    };

                default:
                    throw new ApplicationException($"Unrecognised consolidation entity type {e}");
            }
        }

        public static bool RequiresFederation(Entity entity)
        {
            // all entities currently require federation...
            return entity != Entity.Unknown;
        }

        public static Entity FromString(string s, Entity defValue = Entity.Unknown)
        {
            if (string.IsNullOrEmpty(s))
            {
                return defValue;
            }

            try
            {
                Entity e = (Entity)Enum.Parse(typeof(Entity), s, true);
                return e;
            }
            catch (Exception)
            {
                return defValue;
            }
        }

        public static bool HasUniqueNameColumn(Entity et)
        {
            switch (et)
            {
                case Entity.Equip:
                case Entity.Group:
                case Entity.Module:
                case Entity.Staff:
                case Entity.Room:
                case Entity.Student:
                case Entity.Team:
                case Entity.EsExam:
                    return true;

                default:
                    return false;
            }
        }

        public static string GetFederatedFieldName(string colName)
        {
            return string.Concat(FederationColPrefix, colName);
        }

        public static string GetConsolidatedFieldName(string colName)
        {
            return string.Concat(ConsolidationColPrefix, colName);
        }

        public static bool IsFederatedColumn(string colName)
        {
            return colName.StartsWith(FederationColPrefix);
        }

        public static string GetBaseFieldNameFromFederatedField(string columnName)
        {
            Debug.Assert(IsFederatedColumn(columnName));
            return columnName.Substring(FederationColPrefix.Length);
        }

        public static Entity FromCt7Entity(Ct7Entity e)
        {
            switch (e)
            {
                case Ct7Entity.Course:
                    return Entity.Course;
                case Ct7Entity.Module:
                    return Entity.Module;
                case Ct7Entity.Group:
                    return Entity.Group;
                case Ct7Entity.Staff:
                    return Entity.Staff;
                case Ct7Entity.Room:
                    return Entity.Room;
                case Ct7Entity.Student:
                    return Entity.Student;
                case Ct7Entity.Equipment:
                    return Entity.Equip;
                case Ct7Entity.Team:
                    return Entity.Team;
                case Ct7Entity.Faculty:
                    return Entity.Faculty;
                case Ct7Entity.Department:
                    return Entity.Dept;
                case Ct7Entity.Fixture:
                    return Entity.Fixture;
                case Ct7Entity.Layout:
                    return Entity.Layout;
                case Ct7Entity.Site:
                    return Entity.Site;
                case Ct7Entity.Span:
                    return Entity.Span;
                case Ct7Entity.Origin:
                    return Entity.Origin;
                case Ct7Entity.Event:
                    return Entity.Event;
                case Ct7Entity.EventCat:
                    return Entity.EventCat;
                case Ct7Entity.User:
                    return Entity.User;
                case Ct7Entity.WeekScheme:
                    return Entity.WeekScheme;
                case Ct7Entity.Exam:
                    return Entity.EsExam;
                case Ct7Entity.Session:
                    return Entity.EsSession;
                case Ct7Entity.Slot:
                    return Entity.EsSlot;
                case Ct7Entity.Supervisor:
                    return Entity.Supervisor;
                case Ct7Entity.StaffCat:
                    return Entity.StaffCat;
                case Ct7Entity.Mark:
                    return Entity.AtMark;

                default:
                case Ct7Entity.AuxMark:
                case Ct7Entity.MultiSelect:
                case Ct7Entity.DefaultAccessRights:
                case Ct7Entity.Template:
                case Ct7Entity.Constraint:
                case Ct7Entity.Terminology:
                case Ct7Entity.TemplateStaffEntry:
                case Ct7Entity.TemplateRoomEntry:
                case Ct7Entity.Charge:
                case Ct7Entity.Role:
                case Ct7Entity.ExamSet:
                case Ct7Entity.ConstraintDef:
                case Ct7Entity.GroupSplit:
                case Ct7Entity.ConstraintGenre:
                case Ct7Entity.EventStore:
                case Ct7Entity.Undefined:
                    return Entity.Unknown;
            }
        }
    }
}
