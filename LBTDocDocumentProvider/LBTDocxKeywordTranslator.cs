using System.Globalization;
using System.Threading;

namespace LBTDocDocumentProvider
{
    using System.Collections.Generic;
    using LBT_Repository_1909;
    using System;
    using System.Linq.Dynamic;
    using System.Linq;
    using LBTUnityT4Linq2SqlOrm;
    using System.Reflection;
    using System.Text;
    using CSScriptLibrary;

    public class LBTDocxKeywordTranslator : BaseKeywordTranslator, IDisposable
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly string entityAssemblyName;

        public LBTDocxKeywordTranslator(IUnitOfWork unitOfWork, string entityAssemblyName)
        {
            this.unitOfWork = unitOfWork;
            this.entityAssemblyName = entityAssemblyName;
        }

        public override FieldCollection TranslateKeyword(IDictionary<string, object> inboundParameters, string[] keywords)
        {
            var lbtParameters = GetLbtParameters(keywords);
            var fieldCollection = new FieldCollection();
            foreach (var lbtParameter in lbtParameters)
            {
               
                switch (lbtParameter.ParameterType)
                {
                    case ParameterType.Where:

                        fieldCollection.Add(new Field { Name = lbtParameter.Keyword, Value = WhereStatement(lbtParameter, inboundParameters[lbtParameter.InBoundParameterName]) });
                        break;
                    case ParameterType.Join:
                        fieldCollection.Add(new Field { Name = lbtParameter.Keyword, Value = JoinStatement(lbtParameter, inboundParameters[lbtParameter.InBoundParameterName]) });
                        break;
                    case ParameterType.SqlScript:
                        fieldCollection.Add(new Field { Name = lbtParameter.Keyword, Value = CustomSqlStatment(lbtParameter, inboundParameters[lbtParameter.InBoundParameterName]) });
                        break;
                    case ParameterType.Script:
                        fieldCollection.Add(new Field { Name = lbtParameter.Keyword, Value = MethodStatement(lbtParameter, inboundParameters[lbtParameter.InBoundParameterName]) });
                        break;
                }
            }
            return fieldCollection;
        }

        protected override IEnumerable<ILBTParameter> GetLbtParameters(params string[] fieldName)
        {
            var po =
                unitOfWork.Repository.GetAll(typeof(unt_MuzekkereParametreler))
                    .Cast<unt_MuzekkereParametreler>()
                    .Select(s =>
                        new
                            Tuple<LBTParameter>(
                            new LBTParameter(s.KeyWord.Trim(), s.InboundParameter.Trim(), s.KaynakTablo.Trim(), s.KaynakAlan.Trim(), s.HedefTablo.Trim(), s.HedefAlan.Trim(),
                                s.CustomPostFormula.Trim(), s.CustomWhereCondition.Trim(), s.CustomSQL.Trim(), ((ParameterType)s.Tip), s.Format.Trim())).Item1)
                                .ToList()
                                .Where(w => fieldName.SingleOrDefault(s => w.Keyword.Equals(s)) != null).ToList();
            return po;
        }

        protected override string WhereStatement(ILBTParameter parameter, object inBoundParameterValue)
        {
            var selectState = string.Format("new({0})", parameter.TargetField);
            //var entityType = Type.GetType(parameter.Source);
            var entityType = Assembly.Load(new AssemblyName(entityAssemblyName)).GetTypes().First(f => f.Name.StartsWith(parameter.Target));
            var objectSet = unitOfWork.Repository.GetAll(entityType);
            var resultSet = objectSet.Where(string.Format("{0}=@0", parameter.InBoundParameterName), new[] { inBoundParameterValue });
            resultSet = resultSet.Select(selectState);
            return resultSet.WriteResult(parameter.Format);
        }

        protected override string JoinStatement(ILBTParameter parameter, object inBoundParameterValue)
        {
            var selectState = string.Format("new({0})", parameter.TargetField);
            var preTableKeys = parameter.SourceField.Trim().Split(',');
            var outerKeyLamda = string.Format("o.{0}", preTableKeys[0]);
            var innerKeyLamda = string.Format("i.{0}", preTableKeys[1]);
            var outerType = Assembly.Load(new AssemblyName(entityAssemblyName)).GetTypes().First(f => f.Name.StartsWith(parameter.Source));
            var innerType = Assembly.Load(new AssemblyName(entityAssemblyName)).GetTypes().First(f => f.Name.StartsWith(parameter.Target));
            var outerCollection = unitOfWork.Repository.GetAll(outerType);
            var innerCollection = unitOfWork.Repository.GetAll(innerType);
            var resultCollection = outerCollection.Join("o", innerCollection, "i", outerKeyLamda, innerKeyLamda, selectState);
            resultCollection = resultCollection.Where(parameter.WhereCondition, inBoundParameterValue);
            return resultCollection.WriteResult(parameter.Format);
        }

        protected override string CustomSqlStatment(ILBTParameter parameter, object inBoundParameterValue)
        {
            var targetPorpertyValue = unitOfWork.Repository.GetCustomQuery(parameter.CustomSqlScript, inBoundParameterValue);
            var valueBuilder = new StringBuilder();
            foreach (var value in targetPorpertyValue)
                valueBuilder.AppendLine(value);
            return valueBuilder.ToString();
        }

        protected override string MethodStatement(ILBTParameter parameter, object inBoundParameterValue)
        {
            dynamic script = CSScript.Evaluator.LoadCode(parameter.Script);
            return script.GetDate(parameter);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool p)
        {
            if (unitOfWork != null)
                unitOfWork.Dispose();
        }
    }
}
