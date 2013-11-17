using System.Collections.Generic;
using System.IO;

namespace LBTDocDocumentProvider
{
    public interface IDocumentProvider
    {
        string ProviderName { get; }

        byte[] Render(FieldCollection fields, byte[] template);
    }
}
