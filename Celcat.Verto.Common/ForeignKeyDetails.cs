namespace Celcat.Verto.Common
{
    public class ForeignKeyDetails
    {
        public string FkConstraintName { get; set; }

        public string FkTableName { get; set; }
        
        public string FkColumnName { get; set; }
        
        public int FkPosition { get; set; }

        public string ReferencedConstraintName { get; set; }
        
        public string ReferencedTableName { get; set; }
        
        public string ReferencedColumnName { get; set; }
        
        public int ReferencedPosition { get; set; }
    }
}
