using AWorkFlow2.Models.Working;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AWorkFlow2.Providers
{
    /// <summary>
    /// provides argument managerment, format expression with data
    /// </summary>
    public class ArgumentProvider
    {
        internal WorkingArguments WorkingArguments { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="workingArguments"></param>
        public ArgumentProvider(WorkingArguments workingArguments)
        {
            if (workingArguments != null)
            {
                WorkingArguments = workingArguments;
            }
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
                res = res.Replace(key.Expression, GetValue(key));
            }
            return res;
        }

        /// <summary>
        /// update public argument
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void PutPublic(string key, string value)
        {
            WorkingArguments.PublicArguments[key] = value;
        }

        /// <summary>
        /// update private argument
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void PutPrivate(string key, string value)
        {
            WorkingArguments.PrivateArguments[key] = value;
        }

        /// <summary>
        /// get argument
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            var res = GetPublic(key);
            if (res == null)
            {
                return GetPrivate(key);
            }
            else
            {
                return res;
            }
        }

        /// <summary>
        /// get argument
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetPublic(string key)
        {
            if (WorkingArguments.PublicArguments.ContainsKey(key))
            {
                return WorkingArguments.PublicArguments[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// get argument
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetPrivate(string key)
        {
            if (WorkingArguments.PrivateArguments.ContainsKey(key))
            {
                return WorkingArguments.PrivateArguments[key];
            }
            else
            {
                return null;
            }
        }

        private string GetValue(ExpressionModel key)
        {
            try
            {
                var res = Get(key.Key);
                if (res != null)
                {
                    return res;
                }

                string currentKey = key.CurrentKey;
                if (key.IsArray)
                {
                    currentKey = key.ArrayKey;
                }
                res = Get(currentKey);
                if (res == null)
                {
                    return key.Key;
                }

                var arg = res;
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
                        var token = arr[key.Index].SelectToken(key.SubExpression.Key);
                        return token?.ToString();
                    }
                    else
                    {
                        JObject o = JObject.Parse(arg);
                        var token = o.SelectToken(key.SubExpression.Key);
                        return token?.ToString();
                    }
                }
            }
            catch
            {
                return key.Key;
            }
        }

        /// <summary>
        /// clear specific key
        /// </summary>
        /// <param name="key"></param>
        public void ClearKey(string key)
        {
            if (WorkingArguments.PublicArguments.ContainsKey(key))
            {
                WorkingArguments.PublicArguments.Remove(key);
            }
            if (WorkingArguments.PrivateArguments.ContainsKey(key))
            {
                WorkingArguments.PrivateArguments.Remove(key);
            }
        }
    }
}
