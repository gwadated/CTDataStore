namespace Celcat.Verto.DataStore.Admin.Federation
{
    using System;
    using Celcat.Verto.DataStore.Admin.Federation.Tables;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.TableBuilder;

    internal class FederationTablesBuilder : Builder
    {
        public FederationTablesBuilder()
        {
            // create an instance of each table and add to the table builder...
            foreach (Entity e in Enum.GetValues(typeof(Entity)))
            {
                if (EntityUtils.RequiresFederation(e))
                {
                    AddTable(new FederationTable(EntityUtils.ToFederationTableName(e)));
                }
            }
        }
    }
}
