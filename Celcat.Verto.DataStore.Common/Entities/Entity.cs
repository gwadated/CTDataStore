namespace Celcat.Verto.DataStore.Common.Entities
{
    // NB - naming of entities is important. The CT core tables can be 
    // derived from the name by prefixing "CT_" and inserting an underscore 
    // before Upper case characters
    public enum Entity
    {
        Unknown,

        // the following entities participate in consolidation across timetables
        // (see EntityUtils.CanParticipateInConsolidation)...
        Course,
        Module,
        Group,
        Staff,
        Room,
        Student,
        Equip,
        Team,
        Faculty,
        Dept,
        Fixture,
        Layout,
        Site,
        EventCat,
        Supervisor,
        StaffCat,
        User,

        // the following entities are _not_ consolidated across timetables
        // but do receive a master CTDS Id...
        AtActivity,
        AtAttend,
        AtAttendTime,
        AtException,
        AtMark,
        AtNotification,
        AtStudentException,
        Booking,
        Config,
        EsExam,
        EsSession,
        EsSlot,
        Event,
        Origin,
        Span,
        WeekScheme
    }
}
