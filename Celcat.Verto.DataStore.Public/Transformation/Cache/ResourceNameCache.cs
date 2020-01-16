namespace Celcat.Verto.DataStore.Public.Transformation.Cache
{
    using System.Collections.Generic;

    internal class ResourceNameCache
    {
        private readonly Dictionary<long, ResourceName> _names;

        public ResourceNameCache()
        {
            _names = new Dictionary<long, ResourceName>();
        }

        public void Add(long id, string name, string uniqueName = "")
        {
            _names[id] = new ResourceName
            {
                UniqueName = uniqueName,
                Name = name
            };
        }

        public ResourceName Get(long id)
        {
            _names.TryGetValue(id, out var result);
            return result;
        }
    }
}
