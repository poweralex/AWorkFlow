using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mcs.SF.WorkFlow.Api.Providers
{
    /// <summary>
    /// argument provider
    /// </summary>
    public class ArgumentProvider
    {
        internal Dictionary<string, string> Arguments { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="arguments"></param>
        public ArgumentProvider(Dictionary<string, string> arguments)
        {
            Arguments = arguments;
        }

        /// <summary>
        /// format expression with arguments
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Format(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            var keyExpressions = ExpressionProvider.GetExpressions(input);
            string res = input;
            foreach (var key in keyExpressions)
            {
                res = res.Replace(key.Expression, GetValue(Arguments, key));
            }
            return res;
        }

        /// <summary>
        /// update argument
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Put(string key, string value)
        {
            Arguments[key] = value;
        }

        /// <summary>
        /// get argument
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            if (Arguments.ContainsKey(key))
            {
                return Arguments[key];
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<string> GetArgumentKeys(string input)
        {
            if (input.IndexOf("{key}", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return new List<string> { "{key}" };
            }
            return new List<string> { };
        }

        private string GetValue(Dictionary<string, string> arguments, ExpressionModel key)
        {
            if (arguments.ContainsKey(key.Key))
            {
                return arguments[key.Key];
            }

            string currentKey = key.CurrentKey;
            if (key.IsArray)
            {
                currentKey = key.ArrayKey;
            }
            if (arguments.ContainsKey(currentKey))
            {
                var arg = arguments[currentKey];
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
