using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace FoxTunes
{
    public static class Serializer
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static void Save(Stream stream, IEnumerable<ConfigurationSection> sections)
        {
            using (var writer = new XmlTextWriter(stream, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement(Publication.Product);
                foreach (var section in sections)
                {
                    var elements = section.Elements.Where(
                        element => element.IsModified
                    ).ToArray();
                    if (!elements.Any())
                    {
                        continue;
                    }
                    Logger.Write(typeof(Serializer), LogLevel.Trace, "Saving configuration section: \"{0}\".", section.Id);
                    writer.WriteStartElement(nameof(ConfigurationSection));
                    writer.WriteAttributeString(nameof(section.Id), section.Id);
                    foreach (var element in elements)
                    {
                        Logger.Write(typeof(Serializer), LogLevel.Trace, "Saving configuration element: \"{0}\".", element.Id);
                        writer.WriteStartElement(nameof(ConfigurationElement));
                        writer.WriteAttributeString(nameof(ConfigurationElement.Id), element.Id);
                        writer.WriteCData(element.GetPersistentValue());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public static IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>> Load(Stream stream)
        {
            var sections = new List<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>>();
            using (var reader = new XmlTextReader(stream))
            {
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                reader.ReadStartElement(Publication.Product);
                while (reader.IsStartElement(nameof(ConfigurationSection)))
                {
                    var id = reader.GetAttribute(nameof(ConfigurationSection.Id));
                    Logger.Write(typeof(Serializer), LogLevel.Trace, "Loading configuration section: \"{0}\".", id);
                    reader.ReadStartElement(nameof(ConfigurationSection));
                    sections.Add(new KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>(id, Load(reader)));
                    if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, nameof(ConfigurationSection)))
                    {
                        reader.ReadEndElement();
                    }
                }
                if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, Publication.Product))
                {
                    reader.ReadEndElement();
                }
            }
            return sections;
        }

        private static IEnumerable<KeyValuePair<string, string>> Load(XmlReader reader)
        {
            var elements = new List<KeyValuePair<string, string>>();
            while (reader.IsStartElement(nameof(ConfigurationElement)))
            {
                var id = reader.GetAttribute(nameof(ConfigurationElement.Id));
                Logger.Write(typeof(Serializer), LogLevel.Trace, "Loading configuration element: \"{0}\".", id);
                reader.ReadStartElement(nameof(ConfigurationElement));
                elements.Add(new KeyValuePair<string, string>(id, reader.Value));
                reader.Read(); //Done with <![CDATA[
                if (reader.NodeType == XmlNodeType.EndElement && string.Equals(reader.Name, nameof(ConfigurationElement)))
                {
                    reader.ReadEndElement();
                }
            }
            return elements;
        }
    }
}
