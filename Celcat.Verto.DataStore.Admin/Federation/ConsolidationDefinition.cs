namespace Celcat.Verto.DataStore.Admin.Federation
{
    using Celcat.Verto.DataStore.Common.Consolidation;

    public class ConsolidationDefinition
    {
        public string OriginalColName { get; set; }
        
        public ConsolidationType ConsolidationType { get; set; }
    
        public string ConsolidationIdColName { get; set; }

        public string EntityDefinitionColName { get; set; }
    }
}
