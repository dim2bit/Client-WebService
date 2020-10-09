using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;


namespace WebService
{
    class Serializator
    {
        public static XmlDocument SerializeToXml(Message message)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Message));
            XmlDocument document = null;

            using (MemoryStream memStream = new MemoryStream())
            {
                xs.Serialize(memStream, message);
                memStream.Position = 0;

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (XmlReader xmlreader = XmlReader.Create(memStream, settings))
                {
                    document = new XmlDocument();
                    document.Load(xmlreader);
                }

            }

            return document;
        }

        public static string SerializeToJson(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }
    }
}
