namespace Celcat.Verto.DataStore.Common.Columns
{
    using System;

    public static class ColumnConstants
    {
        public const int StrLenStd = 128;
        public const int StrLenPhotoFile = 255;
        public const int StrLenStaffStudentTitle = 32;
        public const int StrLenComments = 255; // actually bigger than current CT7 implementation (future proof)
        public const int StrLenDescription = 255; // actually bigger than current CT7 implementation (future proof)
        public const int StrLenLookup = 256; // actually bigger than current CT7 implementation (future proof)
        public const int StrLenOriginalId = 255; // actually bigger than current CT7 implementation (future proof)
        public const int StrLenNaturalKey = 255; // actually bigger than current CT7 implementation (future proof)
        public const int StrLenTel = 128;   // actually bigger than current CT7 implementation (future proof)
        public const int StrLenWeb = 255;   // actually bigger than current CT7 implementation (future proof)
        public const int StrLenMax = int.MaxValue;    // equates to "max" as in nvarchar(max)
        public const int StrLenAcademicYear = 64; // actually bigger than current CT7 implementation (future proof)
        public const int MaxWeeks = 56;
        public const string SrcTimetableIdColumnName = "src_timetable_id";
        public const string RegistersReqResolvedColumnName = "registers_req_resolved";
        public const int StrLenEventInstance = 30;
    }
}
