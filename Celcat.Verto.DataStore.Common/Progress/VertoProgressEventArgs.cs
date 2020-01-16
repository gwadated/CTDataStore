namespace Celcat.Verto.DataStore.Common.Progress
{
    using System;

    public class VertoProgressEventArgs : EventArgs
    {
        public string ProgressString { get; set; }
    
        public ProcessingSection Section { get; set; }
    }
}