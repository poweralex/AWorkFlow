using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AWorkFlow.Core.Providers
{
    class ExpressionProvider : IExpressionProvider
    {
        public ArgumentsDto Arguments { get; private set; }

        public ExpressionProvider(ArgumentsDto arguments)
        {
            Arguments = arguments;
        }

        public ExpressionResultDto Format(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return new ExpressionResultDto { IsEmpty = true };
            }
            var keyExpressions = GetExpressions(expression);
            string res = expression;
            foreach (var key in keyExpressions)
            {
                res = res.Replace(key.Expression, GetValue(Arguments, key));
            }
            return new ExpressionResultDto { Result = res };
        }

        /// <summary>
        /// take expression(s) from input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private IEnumerable<ExpressionDto> GetExpressions(string input)
        {
            var result = new List<ExpressionDto>();
            MatchCollection matchs = Regex.Matches(input, @"\{\{.*?\}\}");
            foreach (var m in matchs)
            {
                var expression = m.ToString();
                result.Add(new ExpressionDto(expression, expression.Substring(2, expression.Length - 4)));
            }

            return result;
        }

        private string GetValue(ArgumentsDto arguments, ExpressionDto key)
        {
            if (arguments.ContainsKey(key.Key))
            {
                return arguments.Get(key.Key);
            }

            string currentKey = key.CurrentKey;
            if (key.IsArray)
            {
                currentKey = key.ArrayKey;
            }
            if (arguments.ContainsKey(currentKey))
            {
                var arg = arguments.Get(currentKey);
                if (key.SubExpression == null)
                {
                    if (key.IsArray)
                    {
                        JArray arr = JArray.Parse(arg);
                        if (key.Index >= arr.Count)
                        {
                            return string.Empty;
                        }
                        else
                        {
                            return JsonConvert.SerializeObject(arr[key.Index]);
                        }
                    }
                    else
                    {
                        return arg;
                    }
                }
                else
                {
                    if (key.IsArray)
                    {
                        JArray arr = JArray.Parse(arg);
                        var res = arr[key.Index].SelectToken(key.SubExpression.Key);
                        return res?.ToString();
                    }
                    else
                    {
                        JObject o = JObject.Parse(arg);
                        var res = o.SelectToken(key.SubExpression.Key);
                        return res?.ToString();
                    }
                }
            }

            return key.Key;
        }

    }
}
