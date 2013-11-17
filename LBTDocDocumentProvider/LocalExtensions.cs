using System.IO;
using System.Xml;
using System.Xml.Linq;

using DocumentFormat.OpenXml.Packaging;

namespace LBTDocDocumentProvider
{
    public static class LocalExtensions
    {
        public static XDocument GetXDocument(this OpenXmlPart part)
        {
            var xdoc = part.Annotation<XDocument>();
            if (xdoc != null)
                return xdoc;
            using (var sr = new StreamReader(part.GetStream()))
            using (var xr = XmlReader.Create(sr))
                xdoc = XDocument.Load(xr);
            part.AddAnnotation(xdoc);
            return xdoc;
        }

        public static void PutXDocument(this OpenXmlPart part)
        {
            var xdoc = part.GetXDocument();
            if (xdoc != null)
            {
                // Serialize the XDocument object back to the package.
                using (var xw = XmlWriter.Create(part.GetStream
                    (FileMode.Create, FileAccess.Write)))
                {
                    xdoc.Save(xw);
                }
            }
        }

       
    }
}
