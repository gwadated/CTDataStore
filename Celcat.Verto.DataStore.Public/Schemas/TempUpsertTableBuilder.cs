using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celcat.Verto.Common;
using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
using Celcat.Verto.TableBuilder;
using Celcat.Verto.TableBuilder.Columns;

namespace Celcat.Verto.DataStore.Public.Schemas
{
   class TempUpsertTableBuilder : Builder
   {
      public TempUpsertTableBuilder(string tableName, PublicTable publicTable)
      {
         AddTable(new Table(tableName, publicTable));
      }
   }
}
