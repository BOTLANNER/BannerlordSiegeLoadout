using System;
using System.Xml;

namespace SiegeLoadout
{
    public static class ResourcePrefab
    {
        public static XmlDocument Load(string embedPath)
        {
            using var stream = typeof(ResourcePrefab).Assembly.GetManifestResourceStream(embedPath);
            if (stream is null)
                throw new NullReferenceException($"Could not find embed resource '{embedPath}'!");
            using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true });
            var doc = new XmlDocument();
            doc.Load(xmlReader);
            return doc;
        }
    }
}
