using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AWorkFlow.Core.Models
{
    public class ExpressionResultDto
    {
        public string Result { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsArray { get; set; }

        public T GetResult<T>()
        {
            if (string.IsNullOrEmpty(Result))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(Result);
        }

        public IEnumerable<T> GetArray<T>()
        {
            JArray arr = JArray.Parse(Result);
            return arr.Values<T>();
        }

        public IEnumerable<string> GetArray()
        {
            JArray arr = JArray.Parse(Result);
            return arr.Values<string>();
        }
    }
}
