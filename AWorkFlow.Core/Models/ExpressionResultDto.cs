using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AWorkFlow.Core.Models
{
    public class ExpressionResultDto
    {
        public string ResultJson { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsArray { get; set; }

        public T GetResult<T>()
        {
            return JsonConvert.DeserializeObject<T>(ResultJson);
        }

        public IEnumerable<T> GetArray<T>()
        {
            JArray arr = JArray.Parse(ResultJson);
            return arr.Values<T>();
        }

        public IEnumerable<string> GetArray()
        {
            JArray arr = JArray.Parse(ResultJson);
            return arr.Values<string>();
        }
    }
}
