using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace MidPointUpdatingService
{ 
    public static class XmlDocumentParsingHelper
    {
        public static XmlDocument CreateXMLDocumentFromString(string xmlobj, bool useDTD)
        {
            XmlReader reader;
            XmlDocument doc = new XmlDocument();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = useDTD? DtdProcessing.Parse:DtdProcessing.Ignore;

            try
            {
                reader = XmlReader.Create(new StringReader(xmlobj), settings);
                doc.Load(reader);
                reader.Close();
            }
            catch {}
            return doc;
        }

    }
}
