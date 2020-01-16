namespace Celcat.Verto.DataStore.Common.Progress
{
    public enum ProcessingSection
    {
        Unknown,
        Root,
        Staging,
        UpdatingHistory,
        FederatingResources,
        ConsolidatingResources,
        StagingPublic,
        TransformingPublic
    }
}
