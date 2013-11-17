using System.Collections.Generic;
using System.IO;

namespace LBTDocDocumentProvider
{
    public class HtlmDocumentProviderImpl : BaseDocumentProvider
    {
        #region Overrides of BaseDocumentProvider

        public override string ProviderName
        {
            get { return "HTML Document Provider"; }
        }

        public override byte[] Render(FieldCollection fields, byte[] template)
        {
            throw new System.NotImplementedException();
        }

        public override FieldCollection GetFields(Stream document, string regEx)
        {
            throw new System.NotImplementedException();
        }

        public override byte[] Merge(IEnumerable<byte[]> documents)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}