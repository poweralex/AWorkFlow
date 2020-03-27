using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AWorkFlow2.Providers
{
    /// <summary>
    /// expression provider
    /// </summary>
    public static class ExpressionProvider
    {
        /// <summary>
        /// take expression(s) from input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<ExpressionModel> GetExpressions(string input)
        {
            var result = new List<ExpressionModel>();
            MatchCollection matchs = Regex.Matches(input, @"\{\{.*?\}\}");
            foreach (var m in matchs)
            {
                var expression = m.ToString();
                result.Add(new ExpressionModel(expression, expression.Substring(2, expression.Length - 4)));
            }

            return result;
        }
    }

    /// <summary>
    /// expression model
    /// </summary>
    public class ExpressionModel
    {
        /// <summary>
        /// whole expression
        /// </summary>
        public string Expression { get; set; }
        /// <summary>
        /// whole key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// first key
        /// </summary>
        public string CurrentKey { get; set; }
        /// <summary>
        /// key for array
        /// </summary>
        public string ArrayKey { get; set; }
        /// <summary>
        /// index of array
        /// </summary>
        public int? Index { get; set; }
        /// <summary>
        /// is current key an array
        /// </summary>
        public bool IsArray { get; set; }
        /// <summary>
        /// next expression without current key
        /// </summary>
        public ExpressionModel SubExpression { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="key"></param>
        public ExpressionModel(string expression, string key)
        {
            Expression = expression;
            Key = key;
            var keys = Key.Split('.');
            CurrentKey = keys.FirstOrDefault();
            Match match = Regex.Match(CurrentKey, @"\[\d+\]$");
            IsArray = match.Success;
            if (match.Success)
            {
                Index = int.Parse(match.Value.Substring(1, match.Value.Length - 2));
                ArrayKey = CurrentKey.Replace($"[{Index}]", "");
            }
            if (CurrentKey != Key)
            {
                SubExpression = new ExpressionModel(string.Empty, key.Substring(CurrentKey.Length + 1));
            }
        }
    }
}
