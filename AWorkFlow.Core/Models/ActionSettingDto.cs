using AWorkFlow.Core.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow.Core.Models
{
    /// <summary>
    /// action setting dto
    /// </summary>
    public class ActionSettingDto
    {
        /// <summary>
        /// unique action code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// action type
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// execute sequence
        /// </summary>
        public int Sequence { get; set; }
        /// <summary>
        /// action settings depends on action type
        /// </summary>
        public object Settings { get; set; }
        /// <summary>
        /// result indicator(s)
        /// </summary>
        public List<ResultIndicator> Indicators { get; set; }
    }

    /// <summary>
    /// result indicator setting
    /// </summary>
    public class ResultIndicator
    {
        /// <summary>
        /// actual value
        /// </summary>
        public string ActualExp { get; set; }
        /// <summary>
        /// expected value
        /// </summary>
        public List<string> ExpectedExps { get; set; }
        /// <summary>
        /// logical negation
        /// </summary>
        public bool Not { get; set; }
        /// <summary>
        /// treat as success while compare pass
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// treat as fail while compare pass
        /// </summary>
        public bool IsFail { get; set; }

        /// <summary>
        /// execute indicator
        /// </summary>
        /// <param name="expressionProvider"></param>
        /// <returns></returns>
        public bool? Indicate(IExpressionProvider expressionProvider)
        {
            var actual = expressionProvider.Format(ActualExp).Result;
            var expected = ExpectedExps.Select(x => expressionProvider.Format(x).Result);

            var match = expected.Any(x => string.Equals(actual.ResultJson, x.ResultJson, StringComparison.CurrentCultureIgnoreCase));
            if (Not)
            {
                match = !match;
            }

            if (match)
            {
                if (IsSuccess)
                    return true;
                else if (IsFail)
                    return false;
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
    }
}
