namespace Celcat.Verto.DataStore.Admin.Control.Mutex
{
    using System;

    internal class ControlMutexStatus
    {
        public Guid MutexValue { get; set; }

        public bool Held => MutexValue != Guid.Empty;

        public string MachineName { get; set; }

        public DateTime Start { get; set; }

        public DateTime Touched { get; set; }

        public ControlMutexStatus()
        {
            Start = DateTime.MinValue;
            Touched = DateTime.MinValue;
            MutexValue = Guid.Empty;
        }

        public override string ToString()
        {
            return Held
               ? $"Mutex held by {MachineName} from {Start.ToShortTimeString()}"
               : "Mutex is not held";
        }
    }
}
