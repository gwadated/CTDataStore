namespace Celcat.Verto.DataStore.Public.Staging
{
    using System;
    using Celcat.Verto.DataStore.Admin.Federation.Tables;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.TableBuilder;

    internal class ConsolidationTablesBuilder : Builder
    {
        public ConsolidationTablesBuilder()
        {
            foreach (Entity e in Enum.GetValues(typeof(Entity)))
            {
                if (EntityUtils.CanParticipateInConsolidation(e))
                {
                    string masterTableName = EntityUtils.ToConsolidationTableName(e);
                    AddTable(new ConsolidationTable(masterTableName, PublicStagingSchema.StagingSchemaName));
                    AddTable(new ConsolidationDetailTable(
                        EntityUtils.ToConsolidationDetailTableName(e), 
                        masterTableName, 
                        PublicStagingSchema.StagingSchemaName, 
                        false));
                }
            }
        }
    }
}
