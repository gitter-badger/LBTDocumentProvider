using System.Collections.Generic;
using System.IO;

namespace LBTDocDocumentProvider
{
    public abstract class BaseDocumentProvider : IDocumentProvider
    {
        #region Implementation of IDocumentProvider

        public abstract string ProviderName { get; }

        public abstract byte[] Render(FieldCollection fields, byte[] template);

        public abstract FieldCollection GetFields(Stream document, string regEx);

        public abstract byte[] Merge(IEnumerable<byte[]> documents);

        #endregion
    }
}