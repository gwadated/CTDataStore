namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StudentTable : V7StagingTable
    {
        public StudentTable(string schemaName)
           : base("CT_STUDENT", schemaName)
        {
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new StringColumn("title", ColumnConstants.StrLenStaffStudentTitle));
            AddColumn(new Ct7SexColumn());
            AddColumn(new DateTimeColumn("dob"));
            AddColumn(ColumnUtils.CreateAddressColumns());
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new Ct7AcademicYearColumn());
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(new BigIntColumn("staff_id"));
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new StringColumn("card_num", ColumnConstants.StrLenStd));
            AddColumn(ColumnUtils.CreateTargetColumns());
            AddColumn(new Ct7SchedulableColumn());
            AddColumn(ColumnUtils.CreateStdTelColumns());
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(new StringColumn("profile", ColumnConstants.StrLenStd));
            AddColumn(new StringColumn("photo_file", ColumnConstants.StrLenPhotoFile));
            AddColumn(ColumnUtils.CreateSpecialNeedsColumns());
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new RoomIdReferenceCheck());
            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new StaffIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
