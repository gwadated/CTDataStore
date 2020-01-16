namespace Celcat.Verto.DataStore.Public.Schemas.Exam.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamTable : PublicExamTable
    {
        public ExamTable()
           : base("EXAM")
        {
            AddColumn(new BigIntColumn("exam_id", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new BigIntColumn("session_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("session_name"));
            AddColumn(new IntColumn("duration"));
            AddColumn(ColumnUtils.CreateEventCatIdAndNameColumns());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new IntColumn("capacity_req"));
            AddColumn(ColumnUtils.CreateDeptIdAndNameColumns());
            AddColumn(ColumnUtils.CreateFacultyIdAndNameColumns());
            AddColumn(new BitColumn("protected"));
            AddColumn(new BitColumn("suspended"));
            AddColumn(new IntColumn("grouping_id"));
            AddColumn(new BitColumn("registers_req", ColumnNullable.True));
            AddColumn(new Ct7BoolColumn(ColumnConstants.RegistersReqResolvedColumnName));
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("exam_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("exam_id");
            m.AddExplicitColumnMappingLookup("federated_exam_id", "exam_id", "name", Entity.EsExam);
            m.AddExplicitColumnMappingLookup("federated_exam_id", "exam_id", "unique_name", Entity.EsExam);
            m.AddFederatedIdMapping("session_id");
            m.AddColumnMappingLookup("session_id", "session_name", Entity.EsSession, c);

            m.AddEventCatIdAndNameMapping(c);
            m.AddDeptIdAndNameMapping(c);
            m.AddFacultyIdAndNameMapping(c);

            m.AddBooleanMapping("protected");
            m.AddBooleanMapping("suspended");
            m.AddBooleanMapping("registers_req");
            m.AddBooleanMapping(ColumnConstants.RegistersReqResolvedColumnName);
            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
