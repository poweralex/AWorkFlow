using AWorkFlow2.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2.Helps
{
    /// <summary>
    /// json helper
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// convert json string to json array object
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JArray GetArray(string jsonString, string key)
        {
            try
            {
                return JArray.Parse(jsonString);
            }
            catch
            {
                throw new System.ArgumentException($"{key} should be a json array");
            }
        }
        /// <summary>
        /// try to convert json string to json array object, without exception
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JArray TryGetArray(string jsonString, string key)
        {
            try
            {
                return GetArray(jsonString, key);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// check if jsonString is a valid json array
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static bool IsArray(string jsonString)
        {
            return TryGetArray(jsonString, string.Empty) != null;
        }
        /// <summary>
        /// convert json string to json object
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JObject GetObject(string jsonString, string key)
        {
            try
            {
                return JObject.Parse(jsonString);
            }
            catch
            {
                throw new System.ArgumentException($"{key} should be a json object");
            }
        }

        /// <summary>
        /// try to convert json string to json object without exception
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JObject TryGetObject(string jsonString, string key)
        {
            try
            {
                return GetObject(jsonString, key);
            }
            catch
            {
                // do not throw
                return null;
            }
        }
        /// <summary>
        /// check if jsonString is a valid json object
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static bool IsObject(string jsonString)
        {
            return TryGetObject(jsonString, string.Empty) != null;
        }

        /// <summary>
        /// find token in jtoken
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="objKey"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static JToken FindToken(JToken obj, string key, string objKey, bool ignoreCase = true)
        {
            if (obj == null)
            {
                throw new ArgumentException($"trying to find {key} in {objKey}, but {objKey} is empty");
            }

            try
            {
                if (ignoreCase)
                {
                    IDictionary<string, JToken> dic;
                    if (obj.Type == JTokenType.String)
                    {
                        dic = JsonConvert.DeserializeObject<IDictionary<string, JToken>>(obj.ToString());
                    }
                    else
                    {
                        dic = new Dictionary<string, JToken>(obj.ToObject<IDictionary<string, JToken>>());
                    }
                    var kvp = dic.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.CurrentCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(kvp.Key))
                    {
                        // must use selectToken from obj to provide an token from this obj
                        return obj.SelectToken(kvp.Key);
                    }
                    else
                    {
                        var keyExpressions = ExpressionProvider.GetExpressions("{{" + key + "}}");
                        if (keyExpressions?.Any() == true)
                        {
                            var keyExpression = keyExpressions?.FirstOrDefault();
                            if (keyExpression?.SubExpression != null)
                            {
                                var token = FindToken(obj, keyExpression?.CurrentKey, objKey, ignoreCase);
                                return FindToken(token, keyExpression.SubExpression.Key, $"{objKey}.{keyExpression.CurrentKey}", ignoreCase);
                            }
                        }
                        return obj.SelectToken(key);
                    }
                }
                else
                {
                    return obj.SelectToken(key);
                }
            }
            catch
            {
                throw new System.ArgumentException($"Cannot find {key} in {objKey}");
            }
        }
    }
}
