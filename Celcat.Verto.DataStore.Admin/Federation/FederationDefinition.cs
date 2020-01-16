namespace Celcat.Verto.DataStore.Admin.Federation
{
    using Celcat.Verto.DataStore.Common.Entities;

    public class FederationDefinition
    {
        public string OriginalColName { get; set; }
    
        public Entity Entity { get; set; }
        
        public string FederationIdColName { get; set; }
        
        public string EntityDefinitionColName { get; set; }
    }
}
