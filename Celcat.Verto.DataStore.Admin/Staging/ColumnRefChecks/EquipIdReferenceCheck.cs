namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class EquipIdReferenceCheck : ColumnReferenceCheck
    {
        public EquipIdReferenceCheck() 
            : base("CT_EQUIP", "equip_id")
        {
        }
    }
}
