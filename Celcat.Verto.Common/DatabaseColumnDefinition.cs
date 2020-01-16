namespace Celcat.Verto.Common
{
    public class DatabaseColumnDefinition
    {
        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public bool Nullable { get; set; }

        public string DataType { get; set; }

        public int CharacterMaxLength { get; set; }
    }
}
