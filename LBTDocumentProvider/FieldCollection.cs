using System.Collections.Generic;

namespace LBTDocDocumentProvider
{
    public class FieldCollection : List<IField>
    {
        public FieldCollection() : base() { }

        public FieldCollection(IEnumerable<Field> fieldCollection)
            : base(fieldCollection) { }
    }
}