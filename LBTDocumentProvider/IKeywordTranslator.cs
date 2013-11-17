
using System.Collections.Generic;

namespace LBTDocDocumentProvider
{
    public interface IKeywordTranslator
    {
        FieldCollection TranslateKeyword(IDictionary<string, object> inboundParameters, string[] keywords);
    }
}
