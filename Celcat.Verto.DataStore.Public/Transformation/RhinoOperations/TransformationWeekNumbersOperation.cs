namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using System.Collections.Generic;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class TransformationWeekNumbersOperation : AbstractOperation
    {
        private readonly bool _transform;

        public TransformationWeekNumbersOperation(PublicTable targetTable, DataStoreConfiguration configuration)
        {
            var colMappings = targetTable.GetColumnMappingsFromStage(configuration);
            bool expandEvents = colMappings != null && colMappings.EventExpansionRequired;
     
            _transform = targetTable.ColumnExists("timetable_week") && !expandEvents;
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            foreach (var r in rows)
            {
                if (_transform)
                {
                    r["timetable_week"] = (int)r["timetable_week"] + 1;
                }

                yield return r;
            }
        }
    }
}
