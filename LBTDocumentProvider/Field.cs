namespace LBTDocDocumentProvider
{
    public class Field : IField
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, Value: {1}", Name, Value);
        }
    }
}