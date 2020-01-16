namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using global::Common.Logging;

    internal class ConfigurationFile
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Write(string filePath, DataStoreConfiguration c)
        {
            using (TextWriter w = new StreamWriter(filePath))
            {
                Write(w, c);
            }
        }

        public void Write(TextWriter stream, DataStoreConfiguration c)
        {
            var s = new XmlSerializer(typeof(DataStoreConfiguration));
            s.Serialize(stream, c);
        }

        public DataStoreConfiguration Read(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                return Read(fs);
            }
        }

        public DataStoreConfiguration Read(Stream stream)
        {
            var s = new XmlSerializer(typeof(DataStoreConfiguration));
            s.UnknownAttribute += XmlUnknownAttribute;
            s.UnknownElement += XmlUnknownElement;
            s.UnknownNode += XmlUnknownNode;

            DataStoreConfiguration result = (DataStoreConfiguration)s.Deserialize(stream);

            if (!result.Consolidation.Enabled)
            {
                result.ClearConsolidationColumns();
            }

            result.CheckValidity();
            return result;
        }

        private void XmlUnknownNode(object sender, XmlNodeEventArgs e)
        {
            _log.ErrorFormat("Unknown Node: {0} - {1}", e.Name, e.Text);
        }

        private void XmlUnknownElement(object sender, XmlElementEventArgs e)
        {
            _log.ErrorFormat("Unknown Element: {0}", e.Element);
        }

        private void XmlUnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            _log.ErrorFormat("Unknown Attribute: {0}={1}", attr.Name, attr.Value);
        }
    }
}
