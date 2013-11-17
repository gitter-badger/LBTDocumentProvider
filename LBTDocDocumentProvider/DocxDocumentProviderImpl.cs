using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Source = OpenXmlPowerTools.Source;

namespace LBTDocDocumentProvider
{
    public class DocxDocumentProviderImpl : BaseDocumentProvider
    {

        #region Overrides of BaseDocumentProvider

        public override string ProviderName
        {
            get { return "DOCX Document Provider"; }
        }



        public override byte[] Render(FieldCollection fields, byte[] template)
        {
            IEnumerable<byte[]> document;
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(template, 0, template.Length);
                document = SplitDocument(memoryStream);
                document = document.ToList().Select(s => Render(s, fields.ToArray())).ToArray();
            }
            return Merge(document);
        }



        public override FieldCollection GetFields(Stream document, string regEx = "[%]([A-Z0-9]*)[%]")
        {
            var regExp = new Regex(regEx, RegexOptions.None);
            IEnumerable<Field> po;
            using (var docxDocument = WordprocessingDocument.Open(document, false))
            {
                po =
                    docxDocument.MainDocumentPart.Document.Descendants()
                        .Where(w => regExp.IsMatch(w.InnerText))
                        .Select(s => new Field() { Name = regExp.Match(s.InnerText).Value }).GroupBy(g => g.Name).Select(s => s.First());
            }
            return new FieldCollection(po);
        }

        public override byte[] Merge(IEnumerable<byte[]> documents)
        {

            var sources = (from document in documents let tmpFileName = Path.GetTempFileName() select new Source(new WmlDocument(tmpFileName, document), false)).ToList();
            return DocumentBuilder.BuildDocument(sources).DocumentByteArray;
        }

        #endregion

        IEnumerable<byte[]> SplitDocument(Stream sourceDocument)
        {
            var openXmlElements = new List<IEnumerable<OpenXmlElement>>();
            using (var document = WordprocessingDocument.Open(sourceDocument, true))
            {
                IEnumerable<int> findIndexed;
                if (!IsPageBreak(document.MainDocumentPart.Document.Body, out findIndexed))
                {
                    openXmlElements.Add(document.MainDocumentPart.Document.Body.ToList());
                    return
                        openXmlElements.Select(openXmlElement => openXmlElements.Any() ? CreateDocXDocument(openXmlElement) : null)
                            .Cast<byte[]>()
                            .ToList();
                }
                var firstIndex = findIndexed.First();
                var lastIndex = findIndexed.Last();
                var tBody = document.MainDocumentPart.Document.Body.ToList();
                var lastTakedItem = 0;
                foreach (var i in findIndexed)
                {
                    if (findIndexed.Count() == 1)
                    {
                        openXmlElements.Add(tBody.Take(i));
                        lastTakedItem = i;
                        tBody = new List<OpenXmlElement>(tBody.Skip(i));
                        openXmlElements.Add(tBody);
                        break;
                    }

                    if (firstIndex == i)
                    {
                        openXmlElements.Add(tBody.Take(i));
                        lastTakedItem = i;
                        tBody = new List<OpenXmlElement>(tBody.Skip(i));
                        continue;
                    }
                    if (lastIndex == i)
                    {
                        openXmlElements.Add(tBody.Take(i - lastTakedItem));
                        tBody = new List<OpenXmlElement>(tBody.Skip(i - lastTakedItem));
                        if (tBody.Count > 0) openXmlElements.Add(tBody);
                        continue;
                    }
                    openXmlElements.Add(tBody.Take(i - lastTakedItem));
                    tBody = new List<OpenXmlElement>(tBody.Skip(i - lastTakedItem));
                }
            }
            return openXmlElements.Select(openXmlElement => openXmlElements.Any() ? CreateDocXDocument(openXmlElement) : null).Cast<byte[]>().ToList();
        }

        bool IsPageBreak(Body body, out IEnumerable<int> pageIndexs)
        {
            pageIndexs = new List<int>();
            var paragraph =
                body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
                    .Where(w => w.Descendants<LastRenderedPageBreak>().Any())
                    .ToList();
            if (paragraph.Any())
            {
                foreach (var p in paragraph)
                {
                    ((List<int>)(pageIndexs)).Add(
                        body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().ToList().IndexOf(p));
                }
            }
            else pageIndexs = new int[] { };
            return paragraph.Any();
        }

        byte[] CreateDocXDocument(IEnumerable<OpenXmlElement> items)
        {
            var file = new MemoryStream();
            using (var document = WordprocessingDocument.Create(file, WordprocessingDocumentType.Document, true))
            {

                document.AddMainDocumentPart();
                document.MainDocumentPart.Document = new Document { Body = new Body() };
                foreach (var item in items) document.MainDocumentPart.Document.Body.Append((OpenXmlElement)item.Clone());
                document.MainDocumentPart.Document.Save();
            }
            return file.ToArray();
        }

        byte[] Render(byte[] templateFile, params IField[] fields)
        {
            byte[] targetFile;
            using (var source = new MemoryStream())
            {
                source.Write(templateFile, 0, templateFile.Length);
                using (var wordDoc = WordprocessingDocument.Open(source, true))
                {
                    if (wordDoc.MainDocumentPart.NumberingDefinitionsPart == null)
                        wordDoc.MainDocumentPart.AddNewPart<NumberingDefinitionsPart>();
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    var newPage = new Paragraph(new Run
                    (new Break() { Type = BreakValues.Page }));
                    body.Append(newPage);

                    var paras = body.Descendants<Text>();
                    foreach (var field in fields)
                    {
                        if (field.Value == null)
                            continue;
                        var value = field.Value.ToString();
                        if (value.Contains("\\r\\n"))
                        {
                            var p = body.Descendants<Paragraph>()
                                .Where(w => w.Descendants<Text>().Where(t => t.Text.Contains(field.Name)).Any());
                            var ctx =
                                value.Split(new[] { "\\r\\n" }, StringSplitOptions.None)
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .Select(s => s.Replace("\\r\\n", "").Trim()).ToList();
                            foreach (var v in ctx)
                            {
                                foreach (var pr in p)
                                {

                                    //var run = new Run(new Text(v));
                                    var lineBreak = new Run(new Break());
                                    if (ctx.Last() != v)
                                    {
                                        pr.Descendants<Run>().First().Append(new Text(v), new Break());
                                        continue;
                                    }
                                    pr.Descendants<Run>().First().Append(new Text(v));

                                }
                            }
                            p.ToList().ForEach(f =>
                            {
                                if (f.Descendants<Text>().Where(w => w.Text.Contains(field.Name)).Any())
                                    f.Descendants<Text>().ToList().ForEach(fw => fw.Text = fw.Text.Replace(field.Name, "").Trim());
                            });
                            break;
                        }
                        paras.Where(w => w.Text.Contains(field.Name)).ToList().ForEach(f => f.Text = f.Text.Replace(f.Text, value.Trim()));
                    }
                    wordDoc.MainDocumentPart.PutXDocument();
                    wordDoc.MainDocumentPart.Document.Save();

                }
                targetFile = source.ToArray();
            }
            return DeleteEmptyPage(targetFile);
            //return targetFile;
        }


        byte[] DeleteEmptyPage(byte[] document)
        {
            using (var sourceDocument = new MemoryStream())
            {
                sourceDocument.Write(document, 0, document.Length);
                using (var wdoc = WordprocessingDocument.Open(sourceDocument, true))
                {

                    MainDocumentPart mainPart = wdoc.MainDocumentPart;
                    var breaks = mainPart.Document.Descendants<Break>().ToList().LastOrDefault();
                    if (breaks != null)
                        breaks.Remove();

                    wdoc.MainDocumentPart.PutXDocument();
                    wdoc.MainDocumentPart.Document.Save();

                }
                return sourceDocument.ToArray();
            }
        }
    }
}