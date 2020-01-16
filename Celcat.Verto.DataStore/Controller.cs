namespace Celcat.Verto.DataStore
{
    using System;
    using Celcat.Verto.Common.TableDiff;
    using Celcat.Verto.DataStore.Admin.Admin;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Progress;
    using Celcat.Verto.DataStore.Public.PublicDB;

    public sealed class Controller
    {
        private readonly DataStoreConfiguration _configuration;

        public Controller(DataStoreConfiguration configuration)
        {
            _configuration = configuration;
        }

        public event EventHandler<VertoProgressEventArgs> ProgressEvent;

        public void Execute()
        {
            OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Executing controller", Section = ProcessingSection.Root });

            var adminDb = new AdminDatabase(_configuration);
            adminDb.ProgressEvent += AdminDbProgressEvent;

            var publicDb = new PublicDatabase(_configuration);
            publicDb.ProgressEvent += PublicDbProgressEvent;

            try
            {
                TruncateOnForcedRebuild(adminDb, publicDb);

                CheckForChangeInConsolidationConfig(adminDb, publicDb, _configuration.Consolidation);
                CheckForIdenticalAppKeys(publicDb, adminDb.ApplicationKey);

                // the order of these tasks is important!
                adminDb.ExtractToStage();
                adminDb.UpdateHistory();
                adminDb.AddHistoryFederationIds(new[] { RowStatus.Deleted }, 1);  // perform _before_ we federate
                adminDb.FederateResources();
                adminDb.ConsolidateResources();
                adminDb.AddHistoryFederationIds(new[] { RowStatus.Updated, RowStatus.Inserted }, 2);

                publicDb.PopulateStage(adminDb.ApplicationKey);
                publicDb.TransformStage();
            }
            finally
            {
                adminDb.FinishUp();
                OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Closing controller", Section = ProcessingSection.Root });
            }
        }

        private void CheckForIdenticalAppKeys(PublicDatabase publicDb, Guid adminAppKey)
        {
            publicDb.CheckIdenticalAppKey(adminAppKey);
        }

        private void CheckForChangeInConsolidationConfig(AdminDatabase adminDb, PublicDatabase publicDb, ConsolidationParams consolidation)
        {
            if (adminDb.ConsolidationConfigChanged(consolidation) && publicDb.TablesExist())
            {
                throw new ApplicationException("Consolidation configuration has changed. Please rebuild the data store by deleting ADMIN and PUBLIC databases");
            }
        }

        private void TruncateOnForcedRebuild(AdminDatabase adminDb, PublicDatabase publicDb)
        {
            if (_configuration.ForceRebuild)
            {
                adminDb.TruncateIfForcedRebuild();
                publicDb.TruncateIfForcedRebuild();
            }
        }

        private void PublicDbProgressEvent(object sender, VertoProgressEventArgs e)
        {
            OnProgressEvent(e);
        }

        private void AdminDbProgressEvent(object sender, VertoProgressEventArgs e)
        {
            OnProgressEvent(e);
        }

        private void OnProgressEvent(VertoProgressEventArgs e)
        {
            ProgressEvent?.Invoke(this, e);
        }
    }
}
