namespace Celcat.Verto.DataStore.Admin.Federation
{
    using System;
    using Celcat.Verto.DataStore.Admin.Federation.Tables;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.TableBuilder;

    internal class ConsolidationTablesBuilder : Builder
    {
        public ConsolidationTablesBuilder()
        {
            // create an instance of each table and add to the table builder...
            AddTable(new ConsolidationConfigTable());

            foreach (Entity e in Enum.GetValues(typeof(Entity)))
            {
                if (EntityUtils.CanParticipateInConsolidation(e))
                {
                    var masterTableName = EntityUtils.ToConsolidationTableName(e);

                    AddTable(new ConsolidationTable(masterTableName, FederationSchema.FederationSchemaName));
                    AddTable(new ConsolidationDetailTable(
                        EntityUtils.ToConsolidationDetailTableName(e),
                        masterTableName, 
                        FederationSchema.FederationSchemaName));
                }
            }
        }
    }
}
