using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBTDocDocumentProvider
{
    public interface ILBTParameter
    {
        string Seperator { get; }

        string Keyword { get; set; }

        string InBoundParameterName { get; set; }

        string Source { get; set; }

        string SourceField { get; set; }

        string Target { get; set; }

        string TargetField { get; set; }

        string Script { get; set; }

        string WhereCondition { get; set; }

        string CustomSqlScript { get; set; }

        string Format { get; set; }

        ParameterType ParameterType { get; }
    }

    [Flags]
    public enum ParameterType : byte
    {
        Where = 0, Join = 1, SqlScript = 2, Script = 3
    }
}
