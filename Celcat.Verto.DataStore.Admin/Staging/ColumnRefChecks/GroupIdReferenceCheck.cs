namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;
    
    internal class GroupIdReferenceCheck : ColumnReferenceCheck
    {
        public GroupIdReferenceCheck() 
            : base("CT_GROUP", "group_id")
        {
        }
    }
}
