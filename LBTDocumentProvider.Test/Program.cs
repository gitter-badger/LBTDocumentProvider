using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LBTDocDocumentProvider;
using LBTDocumentProvider.UnitOfWork;

namespace LBTDocumentProvider.Test
{
    class Program
    {
        static void Main()
        {
            //const string adaletSarayi = "%ADALETSARAYI%";
            //const string icraDairesi = "%ICRADAIRESI%";
            //const string kesinlesenBorclular = "%KESINLESENBORCLULAR%";

            //var docxDocument = new DocxDocumentProviderImpl();
            //var fields = new Field[]
            //{
            //    new Field() { Name = adaletSarayi, Value = "İstanbul" },
            //    new Field() { Name = icraDairesi, Value = 4 },
            //    new Field() { Name = kesinlesenBorclular, Value = "ilker halil türer\\r\\nturkan kabacalman\\r\\n" }
            //};
            //var fieldCollection = new FieldCollection(fields);
            //var template = File.ReadAllBytes("1.docx");
            //var newDocument = docxDocument.Render(fieldCollection, (byte[])template.Clone());

            //File.WriteAllBytes("2.docx", newDocument);

            //var docxDocument1 = new DocxDocumentProviderImpl();
            //var fields1 = new Field[]
            //{
            //    new Field() { Name = adaletSarayi, Value = "Bursa" },
            //    new Field() { Name = icraDairesi, Value = 2 },
            //    new Field() { Name = kesinlesenBorclular, Value = "mustafa sarıel\\r\\nsedat ersel\\r\\n" }
            //};
            //var fieldCollection1 = new FieldCollection(fields1);
            ////var template1 = File.ReadAllBytes("1.docx");
            //var newDocument1 = docxDocument.Render(fieldCollection1, (byte[])template.Clone());

            //File.WriteAllBytes("3.docx", newDocument1);
            //var owner = File.ReadAllBytes("2.docx");
            //var chiled1 = File.ReadAllBytes("3.docx");
            //var childs = new List<byte[]> { chiled1 };
            //var fs = new FileStream("4.docx", FileMode.OpenOrCreate);
            //docxDocument1.Merge(owner, childs, fs);
            //Process.Start("4.docx");



            var parameters = new Dictionary<string, object>
            {
                { "MuzekkereTip", 82 },
                { "ContactID", 423311 },
                { "ClaimID", 917270 },
                { "LoanID", 263210 }
            };
            var docxDocumentProvider = new DocxDocumentProviderImpl();
            var templateFile = File.ReadAllBytes("1.docx");
            var keywordProc = new LBTDocxKeywordTranslator(new LBTUnityLinq2SqlOrmUnitofWork(), "LBTDocumentProvider.OrmImpl");
            var fieldCollection = docxDocumentProvider.GetFields(new MemoryStream(templateFile));
            fieldCollection = keywordProc.TranslateKeyword(parameters, fieldCollection.Select(s => s.Name).ToArray());

            var docx = docxDocumentProvider.Render(fieldCollection, templateFile);

            var documentName = System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "docx");
            File.WriteAllBytes(documentName, docx);
            Process.Start(documentName);



            Console.ReadKey();

        }

        

    }
}
