using System.Collections.Generic;

namespace LBTDocDocumentProvider
{
    public abstract class BaseKeywordTranslator : IKeywordTranslator
    {

        #region Implementation of IKeywordTranslator

        public abstract FieldCollection TranslateKeyword(IDictionary<string, object> inboundParameters, string[] keywords);

        #endregion

        protected abstract IEnumerable<ILBTParameter> GetLbtParameters(params string[] fieldName);

        protected abstract string WhereStatement(ILBTParameter parameter, object inBoundParameterValue);

        protected abstract string JoinStatement(ILBTParameter parameter, object inBoundParameterValue);

        protected abstract string CustomSqlStatment(ILBTParameter parameter, object inBoundParameterValue);

        protected abstract string MethodStatement(ILBTParameter parameter, object inBoundParameterValue);

    }
}