using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Mcs.SF.WorkFlow.Api.Models.Working
{
    /// <summary>
    /// data model for working variables
    /// </summary>
    public class WorkingVariables
    {
        /// <summary>
        /// key-value pairs
        /// </summary>
        public Dictionary<string, string> VariablesDic { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// init with data
        /// </summary>
        /// <param name="data"></param>
        public void Init(Dictionary<string, string> data)
        {
            VariablesDic = new Dictionary<string, string>(data, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// add item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, object value)
        {
            if (value is string)
            {
                VariablesDic.Add(key, (string)value);
            }
            else
            {
                VariablesDic.Add(key, JsonConvert.SerializeObject(value));
            }
        }

        /// <summary>
        /// retreive item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (VariablesDic.TryGetValue(key, out string res))
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            return default(T);
        }
    }
}
