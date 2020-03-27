using AWorkFlow2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// provide processor for type=VariableProcess and method=Set
    /// </summary>
    public class SetValueProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "Set";
        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<SetValueActionSetting>(actionSetting);
            var results = new Dictionary<string, string>();
            foreach (var kvp in setting.Set)
            {
                var target = argument.Format(kvp.Key);
                ExpressionModel expression = new ExpressionModel(string.Empty, target);
                var currentKey = expression.CurrentKey;
                if (expression.IsArray)
                {
                    currentKey = expression.ArrayKey;
                }
                var obj = argument.Get(currentKey);
                if (obj == null)
                {
                    argument.PutPrivate(target, argument.Format(kvp.Value));
                    results.Add(target, argument.Get(target));
                }
                else
                {
                    JToken token = null;
                    JToken res = null;
                    if (expression.IsArray)
                    {
                        JArray arr = JArray.Parse(obj);
                        res = arr[expression.Index]?.SelectToken(expression.SubExpression.Key);
                        if (res == null)
                        {
                            arr = FillArrayToCount(arr, expression.Index ?? 0);
                            if (arr[expression.Index] == null)
                            {
                                arr[expression.Index] = CreateObject(new JObject(), expression.SubExpression);
                                res = arr[expression.Index]?.SelectToken(expression.SubExpression.Key);
                            }
                        }
                        token = arr;
                    }
                    else
                    {
                        JObject o = JObject.Parse(obj);
                        res = o.SelectToken(expression.SubExpression.Key);
                        if (res == null)
                        {
                            o = CreateObject(o, expression.SubExpression);
                            res = o.SelectToken(expression.SubExpression.Key);
                        }
                        token = o;
                    }

                    if (res?.Parent != null)
                    {
                        var prop = (JProperty)res.Parent;
                        var newValue = argument.Format(kvp.Value);
                        if (newValue.StartsWith("["))
                        {
                            JArray newArr = JArray.Parse(newValue);
                            prop.Value = newArr;
                        }
                        else if (newValue.StartsWith("{"))
                        {
                            JObject newObj = JObject.Parse(newValue);
                            prop.Value = newObj;
                        }
                        else
                        {
                            prop.Value = newValue;
                        }
                    }
                    argument.PutPublic(currentKey, JsonConvert.SerializeObject(token));
                    results.Add(currentKey, argument.Get(expression.CurrentKey));
                }
            }

            return Task.FromResult(new ActionExecuteResult
            {
                Success = true,
                Data = JsonConvert.SerializeObject(results)
            });
        }

        private JObject CreateObject(JObject obj, ExpressionModel expression)
        {
            var baseObj = obj?[expression.CurrentKey];
            JObject res = obj;
            if (res == null)
            {
                res = new JObject();
            }
            JContainer sub = null;
            if (expression.SubExpression != null)
            {
                sub = CreateObject((JObject)baseObj, expression.SubExpression);
            }

            if (expression.IsArray)
            {
                var arr = FillArrayToCount(new JArray(), expression.Index ?? 0);

                if (sub != null)
                {
                    arr[expression.Index] = sub;
                }
                res.Add(expression.ArrayKey, arr);
            }
            else
            {
                if (res[expression.CurrentKey] == null)
                {
                    res.Add(expression.CurrentKey, sub);
                }
                else
                {
                    res[expression.CurrentKey] = sub;
                }
            }

            return res;
        }

        private JArray FillArrayToCount(JArray array, int count)
        {
            if (array == null)
            {
                array = new JArray();
            }
            for (int i = array.Count; i <= count; i++)
            {
                array.Add(new JObject());
            }

            return array;
        }
    }

    /// <summary>
    /// variable process action setting model
    /// </summary>
    public class SetValueActionSetting
    {
        /// <summary>
        /// output setting
        /// </summary>
        public Dictionary<string, string> Set { get; set; }
    }

}
