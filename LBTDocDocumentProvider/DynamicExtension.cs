using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;

namespace LBTDocDocumentProvider
{
    public static class DynamicExtension
    {
        public static IQueryable Join(this IQueryable outer, IEnumerable inner, string outerSelector, string innerSelector, string resultsSelector, params object[] values)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (outerSelector == null) throw new ArgumentNullException("outerSelector");
            if (innerSelector == null) throw new ArgumentNullException("innerSelector");
            if (resultsSelector == null) throw new ArgumentNullException("resultsSelctor");

            var outerSelectorLambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(outer.ElementType, null, outerSelector, values);
            var innerSelectorLambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(inner.AsQueryable().ElementType, null, innerSelector, values);

            var parameters = new ParameterExpression[] {
            Expression.Parameter(outer.ElementType, "outer"), Expression.Parameter(inner.AsQueryable().ElementType, "inner") };
            var resultsSelectorLambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(parameters, null, resultsSelector, values);

            return outer.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Join",
                    new Type[] { outer.ElementType, inner.AsQueryable().ElementType, outerSelectorLambda.Body.Type, resultsSelectorLambda.Body.Type },
                    outer.Expression, inner.AsQueryable().Expression, Expression.Quote(outerSelectorLambda), Expression.Quote(innerSelectorLambda), Expression.Quote(resultsSelectorLambda)));
        }

        public static IQueryable<T> Join<T>(this IQueryable<T> outer, IEnumerable<T> inner, string outerSelector, string innerSelector, string resultsSelector, params object[] values)
        {
            return (IQueryable<T>)Join((IQueryable)outer, (IEnumerable)inner, outerSelector, innerSelector, resultsSelector, values);
        }


        public static IQueryable Join(this IQueryable source1, string alias1, IQueryable source2, string alias2, string key1, string key2, string selector, params object[] args)
        {

            if (source1 == null) throw new ArgumentNullException("source1");

            if (alias1 == null) throw new ArgumentNullException("alias1");

            if (source2 == null) throw new ArgumentNullException("source1");

            if (alias2 == null) throw new ArgumentNullException("alias2");

            if (key1 == null) throw new ArgumentNullException("key1");

            if (key2 == null) throw new ArgumentNullException("key2");

            if (selector == null) throw new ArgumentNullException("selector");

            ParameterExpression p1 = Expression.Parameter(source1.ElementType, alias1);

            ParameterExpression p2 = Expression.Parameter(source2.ElementType, alias2);

            LambdaExpression keyLambda1 = System.Linq.Dynamic.DynamicExpression.ParseLambda(new ParameterExpression[] { p1 }, null, key1, null);

            LambdaExpression keyLambda2 = System.Linq.Dynamic.DynamicExpression.ParseLambda(new ParameterExpression[] { p2 }, null, key2, null);

            FixLambdaReturnTypes(ref keyLambda1, ref keyLambda2);

            LambdaExpression lambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(new ParameterExpression[] { p1, p2 }, null, selector, args);

            return source1.Provider.CreateQuery(

              Expression.Call(

                typeof(Queryable), "Join",

                new Type[] { source1.ElementType, source2.ElementType, keyLambda1.Body.Type, lambda.Body.Type },

               source1.Expression, source2.Expression, Expression.Quote(keyLambda1), Expression.Quote(keyLambda2), Expression.Quote(lambda)

               ));

        }

        private static void FixLambdaReturnTypes(ref LambdaExpression lambda1, ref LambdaExpression lambda2)
        {

            Type type1 = lambda1.Body.Type;

            Type type2 = lambda2.Body.Type;

            if (type1 != type2)
            {

                if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(Nullable<>) && type1 == type2.GetGenericArguments()[0])
                {

                    lambda1 = Expression.Lambda(Expression.Convert(lambda1.Body, type2), lambda1.Parameters.ToArray());

                }

                else
                {

                    // this may fail because types are incompatible

                    lambda2 = Expression.Lambda(Expression.Convert(lambda2.Body, type1), lambda2.Parameters.ToArray());

                }

            }

        }


        public static string WriteResult(this IQueryable resultCollection, string format)
        {

            var properties = new List<string>();
            var op = resultCollection as IEnumerable<DynamicClass>;
            var dynamicClasses = op as DynamicClass[] ?? op.ToArray();
            if (dynamicClasses.Count() == 1)
            {
                return GetDynamicObjectValue(dynamicClasses.First(), format);
            }
            if (dynamicClasses.Count() > 1)
            {
                properties.AddRange(dynamicClasses.Select(item => GetDynamicObjectValue(item, format)));
            }
            var propertyValues = string.Empty;
            properties.ForEach(str => propertyValues += string.Format("{0} ", str));
            return propertyValues;
        }
        private static string GetDynamicObjectValue(DynamicClass dynamicObject, string format)
        {

            var propertyValues = dynamicObject.ToString();
            var propertiesName = dynamicObject.GetType().GetProperties().Select(s => s.Name).ToList();
            propertiesName.ForEach(prop => propertyValues = propertyValues.Replace(prop, " "));
            return string.Format(!string.IsNullOrEmpty(format) ? format : " ", propertyValues.Replace('{', ' ').Replace('}', ' ').Replace('=', ' ').Split(','));
        }

        public static T Parse<T>(this string value)
        {
            var result = default(T);
            if (string.IsNullOrEmpty(value)) return result;
            var tc = TypeDescriptor.GetConverter(typeof(T));
            result = (T)tc.ConvertFrom(value);
            return result;
        }
    }
}
