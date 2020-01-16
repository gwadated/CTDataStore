namespace Celcat.Verto.DataStore.Common.Entities
{
    using System;

    public static class Ct7EntityUtils
    {
        // taken from Ct7 codebase (CTDataDLL...ctEntityType.cpp)
        public static string GetTableName(Ct7Entity et)
        {
            string tableName;

            switch (et)
            {
                case Ct7Entity.Course:
                    tableName = "CT_COURSE";
                    break;
                case Ct7Entity.Module:
                    tableName = "CT_MODULE";
                    break;
                case Ct7Entity.Group:
                    tableName = "CT_GROUP";
                    break;
                case Ct7Entity.Staff:
                    tableName = "CT_STAFF";
                    break;
                case Ct7Entity.Room:
                    tableName = "CT_ROOM";
                    break;
                case Ct7Entity.Student:
                    tableName = "CT_STUDENT";
                    break;
                case Ct7Entity.Equipment:
                    tableName = "CT_EQUIP";
                    break;
                case Ct7Entity.Team:
                    tableName = "CT_TEAM";
                    break;
                case Ct7Entity.Terminology:
                    tableName = "CT_TERMINOLOGY";
                    break;
                case Ct7Entity.Faculty:
                    tableName = "CT_FACULTY";
                    break;
                case Ct7Entity.Department:
                    tableName = "CT_DEPT";
                    break;
                case Ct7Entity.Fixture:
                    tableName = "CT_FIXTURE";
                    break;
                case Ct7Entity.Layout:
                    tableName = "CT_LAYOUT";
                    break;
                case Ct7Entity.Site:
                    tableName = "CT_SITE";
                    break;
                case Ct7Entity.Span:
                    tableName = "CT_SPAN";
                    break;
                case Ct7Entity.Role:
                    tableName = "CT_ROLE";
                    break;
                case Ct7Entity.Origin:
                    tableName = "CT_ORIGIN";
                    break;
                case Ct7Entity.Event:
                    tableName = "CT_EVENT";
                    break;
                case Ct7Entity.EventCat:
                    tableName = "CT_EVENT_CAT";
                    break;
                case Ct7Entity.User:
                    tableName = "CT_USER";
                    break;
                case Ct7Entity.WeekScheme:
                    tableName = "CT_WEEK_SCHEME";
                    break;
                case Ct7Entity.Charge:
                    tableName = "CT_CHARGE";
                    break;
                case Ct7Entity.Exam:
                    tableName = "CT_ES_EXAM";
                    break;
                case Ct7Entity.Session:
                    tableName = "CT_ES_SESSION";
                    break;
                case Ct7Entity.Slot:
                    tableName = "CT_ES_SLOT";
                    break;
                case Ct7Entity.Supervisor:
                    tableName = "CT_SUPERVISOR";
                    break;
                case Ct7Entity.Constraint:
                    tableName = "CT_CS_CONSTRAINT";
                    break;
                case Ct7Entity.Template:
                    tableName = "CT_CS_TEMPLATE";
                    break;
                case Ct7Entity.TemplateStaffEntry:
                    tableName = "CT_CS_STAFF_ENTRY";
                    break;
                case Ct7Entity.TemplateRoomEntry:
                    tableName = "CT_CS_ROOM_ENTRY";
                    break;
                case Ct7Entity.ConstraintDef:
                    tableName = "CT_CS_CONSTRAINT_DEF";
                    break;
                case Ct7Entity.ConstraintGenre:
                    tableName = "CT_CS_CONSTRAINT_GENRE";
                    break;
                case Ct7Entity.GroupSplit:
                    tableName = "CT_CS_GROUPSPLIT";
                    break;
                case Ct7Entity.EventStore:
                    tableName = "CT_EVENT_STORE";
                    break;
                case Ct7Entity.DefaultAccessRights:
                    tableName = "CT_ACCESS_DEF_TT";
                    break;
                case Ct7Entity.MultiSelect:
                    tableName = "CT_MULTI_SELECT";
                    break;
                case Ct7Entity.StaffCat:
                    tableName = "CT_STAFF_CAT";
                    break;
                case Ct7Entity.Mark:
                    tableName = "CT_AT_MARK";
                    break;
                case Ct7Entity.AuxMark:
                    tableName = "CT_AT_AUX_MARK";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(et), "Could not recognise ct7 entity");
            }

            return tableName;
        }

        public static string GetIdFldName(Ct7Entity et)
        {
            string name;

            switch (et)
            {
                case Ct7Entity.Course: name = "course_id"; break;
                case Ct7Entity.Module: name = "module_id"; break;
                case Ct7Entity.Group: name = "group_id"; break;
                case Ct7Entity.Staff: name = "staff_id"; break;
                case Ct7Entity.Room: name = "room_id"; break;
                case Ct7Entity.Student: name = "student_id"; break;
                case Ct7Entity.Equipment: name = "equip_id"; break;
                case Ct7Entity.Team: name = "team_id"; break;
                case Ct7Entity.Terminology: name = "terminology_id"; break;
                case Ct7Entity.Faculty: name = "faculty_id"; break;
                case Ct7Entity.Department: name = "dept_id"; break;
                case Ct7Entity.Fixture: name = "fixture_id"; break;
                case Ct7Entity.Layout: name = "room_layout_id"; break;
                case Ct7Entity.Site: name = "site_id"; break;
                case Ct7Entity.Span: name = "span_id"; break;
                case Ct7Entity.Role: name = "role_id"; break;
                case Ct7Entity.Origin: name = "origin_id"; break;
                case Ct7Entity.Event: name = "event_id"; break;
                case Ct7Entity.EventCat: name = "event_cat_id"; break;
                case Ct7Entity.User: name = "user_id"; break;
                case Ct7Entity.WeekScheme: name = "week_scheme_id"; break;
                case Ct7Entity.Charge: name = "charge_id"; break;
                case Ct7Entity.Exam: name = "exam_id"; break;
                case Ct7Entity.Session: name = "session_id"; break;
                case Ct7Entity.Slot: name = "slot_id"; break;
                case Ct7Entity.Supervisor: name = "supervisor_id"; break;
                case Ct7Entity.ExamSet: name = "exam_set_id"; break;
                case Ct7Entity.Constraint: name = "constraint_id"; break;
                case Ct7Entity.Template: name = "template_id"; break;
                case Ct7Entity.TemplateStaffEntry: name = "staff_entry_id"; break;
                case Ct7Entity.TemplateRoomEntry: name = "room_entry_id"; break;
                case Ct7Entity.ConstraintDef: name = "constraint_def_id"; break;
                case Ct7Entity.ConstraintGenre: name = "constraint_genre_id"; break;
                case Ct7Entity.GroupSplit: name = "split_id"; break;
                case Ct7Entity.EventStore: name = "store_id"; break;
                case Ct7Entity.DefaultAccessRights: name = "resource_type"; break;
                case Ct7Entity.MultiSelect: name = "multi_select_id"; break;
                case Ct7Entity.StaffCat: name = "staff_cat_id"; break;
                case Ct7Entity.Mark: name = "mark_id"; break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(et), "Could not recognise ct7 entity");
            }

            return name;
        }
    }
}
