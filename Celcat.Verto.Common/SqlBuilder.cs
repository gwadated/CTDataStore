namespace Celcat.Verto.Common
{
    using System.Text;

    public sealed class SqlBuilder
    {
        private readonly StringBuilder _stringBuilder;

        public SqlBuilder()
        {
            _stringBuilder = new StringBuilder();
        }

        public bool Empty => Length == 0;

        public int Length => _stringBuilder.Length;

        public SqlBuilder AppendFormat(string format, object arg0)
        {
            _stringBuilder.AppendFormat(format, arg0);
            _stringBuilder.AppendLine();
            return this;
        }

        public SqlBuilder AppendFormat(string format, object arg0, object arg1)
        {
            _stringBuilder.AppendFormat(format, arg0, arg1);
            _stringBuilder.AppendLine();
            return this;
        }

        public SqlBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            _stringBuilder.AppendFormat(format, arg0, arg1, arg2);
            _stringBuilder.AppendLine();
            return this;
        }

        public SqlBuilder AppendFormat(string format, params object[] args)
        {
            _stringBuilder.AppendFormat(format, args);
            _stringBuilder.AppendLine();
            return this;
        }

        public void AppendNoSpace(string sql)
        {
            _stringBuilder.Append(sql);
        }

        public void Append(string sql)
        {
            _stringBuilder.AppendLine(sql);
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }

        public void Clear()
        {
            _stringBuilder.Clear();
        }
    }
}
