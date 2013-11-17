using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LBTDocDocumentProvider
{
    public class LBTParameter : ILBTParameter
    {
        public LBTParameter()
        {
            Seperator = ",";
        }

        public LBTParameter(string keyword, string inBoundParameterName, string source, string sourceField, string target, string targetField, string script, string whereCondition, string customSqlScript, ParameterType parameterType, string format)
        {
            Keyword = keyword;
            InBoundParameterName = inBoundParameterName;
            Source = source;
            SourceField = sourceField;
            Target = target;
            TargetField = targetField;
            Script = script;
            WhereCondition = whereCondition;
            CustomSqlScript = customSqlScript;
            ParameterType = parameterType;
            Format = format;
        }

        #region Implementation of ILBTParameter

        public string Seperator { get; private set; }
        public string Keyword { get; set; }
        public string InBoundParameterName { get; set; }
        public string Source { get; set; }
        public string SourceField { get; set; }
        public string Target { get; set; }
        public string TargetField { get; set; }
        public string Script { get; set; }
        public string WhereCondition { get; set; }
        public string CustomSqlScript { get; set; }
        public string Format { get; set; }
        public ParameterType ParameterType { get; private set; }

        #endregion

        #region KeywordEqualityComparer

        public sealed class KeywordEqualityComparer : IEqualityComparer<LBTParameter>
        {
            public bool Equals(LBTParameter x, LBTParameter y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null))
                {
                    return false;
                }
                if (ReferenceEquals(y, null))
                {
                    return false;
                }
                if (x.GetType() != y.GetType())
                {
                    return false;
                }
                return string.Equals(x.Keyword, y.Keyword);
            }

            public int GetHashCode(LBTParameter obj)
            {
                return (obj.Keyword != null ? obj.Keyword.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<LBTParameter> keywordComparerInstance = new KeywordEqualityComparer();

        public static IEqualityComparer<LBTParameter> KeywordComparer
        {
            get { return keywordComparerInstance; }
        }

        #endregion
    }


}