namespace Celcat.Verto.Common.TableDiff
{
    using System.Text;

    public class DataDiff
    {
        public SimpleTableRow OldRow { get; set; }

        public SimpleTableRow NewRow { get; set; }

        public RowStatus Status
        {
            get
            {
                if (OldRow == null && NewRow == null)
                {
                    return RowStatus.Unknown;
                }

                if (OldRow == null)
                {
                    return RowStatus.Inserted;
                }

                if (NewRow == null)
                {
                    return RowStatus.Deleted;
                }

                return RowStatus.Updated;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("OldRow=");

            if (OldRow == null)
            {
                sb.Append("null");
            }
            else
            {
                sb.Append(OldRow);
            }

            sb.Append(" ");

            sb.Append("NewRow=");

            if (NewRow == null)
            {
                sb.Append("null");
            }
            else
            {
                sb.Append(NewRow);
            }

            return sb.ToString();
        }
    }
}
