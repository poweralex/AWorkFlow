using AWorkFlow2.Helps;
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
        /// <param name="privateFirst"></param>
        /// <returns></returns>
        public string Format(string input, bool privateFirst = false)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            var keyExpressions = ExpressionProvider.GetExpressions(input);
            string res = input;
            foreach (var key in keyExpressions)
            {
                res = res.Replace(key.Expression, GetValue(key, privateFirst));
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
        /// <param name="privateFirst"></param>
        /// <returns></returns>
        public string Get(string key, bool privateFirst)
        {
            if (privateFirst)
            {
                var res = GetPrivate(key);
                if (res == null)
                {
                    return GetPublic(key);
                }
                else
                {
                    return res;
                }
            }
            else
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

        private string GetValue(ExpressionModel key, bool privateFirst)
        {
            try
            {
                var res = Get(key.Key, privateFirst);
                if (res != null)
                {
                    return res;
                }

                string currentKey = key.CurrentKey;
                if (key.IsArray)
                {
                    currentKey = key.ArrayKey;
                }
                res = Get(currentKey, privateFirst);
                if (res == null)
                {
                    return string.Empty;
                }

                var arg = res;
                if (key.SubExpression == null)
                {
                    if (key.IsArray)
                    {
                        JArray arr = JsonHelper.GetArray(arg, currentKey);
                        if (key.Index >= arr.Count)
                        {
                            return string.Empty;
                        }
                        else
                        {
                            return arr[key.Index].ToString();
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
                        JArray arr = JsonHelper.GetArray(arg, currentKey);
                        var token = JsonHelper.FindToken(arr[key.Index], key.SubExpression.Key, currentKey);
                        return token?.ToString();
                    }
                    else
                    {
                        JObject o = JsonHelper.GetObject(arg, currentKey);
                        var token = JsonHelper.FindToken(o, key.SubExpression.Key, currentKey);
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
