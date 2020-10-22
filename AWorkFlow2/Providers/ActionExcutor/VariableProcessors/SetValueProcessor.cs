using AWorkFlow2.Helps;
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
            var results = new Dictionary<string, JToken>();
            foreach (var kvp in setting.Set)
            {
                var target = argument.Format(kvp.Key, true);
                ExpressionModel expression = new ExpressionModel(string.Empty, target);
                var currentKey = expression.CurrentKey;
                if (expression.IsArray)
                {
                    currentKey = expression.ArrayKey;
                }
                var obj = argument.Get(currentKey, true);
                if (obj != null && !(JsonHelper.IsObject(obj) || JsonHelper.IsArray(obj)))
                {
                    obj = null;
                }
                if (obj == null)
                {
                    string targetValue = argument.Format(kvp.Value, true);
                    if (setting.AsString)
                    {
                        results.Add(target, targetValue);
                    }
                    else
                    {
                        var valueObj = JsonHelper.TryGetObject(targetValue, kvp.Value);
                        var valueArr = JsonHelper.TryGetArray(targetValue, kvp.Value);
                        if (valueObj != null)
                        {
                            targetValue = JsonConvert.SerializeObject(valueObj);
                            results.Add(target, valueObj);
                        }
                        else if (valueArr != null)
                        {
                            targetValue = JsonConvert.SerializeObject(valueArr);
                            results.Add(target, valueArr);
                        }
                        else
                        {
                            results.Add(target, targetValue);
                        }
                    }
                    argument.PutPrivate(target, targetValue);
                }
                else
                {
                    JToken token = null;
                    JToken res = null;
                    if (expression.IsArray)
                    {
                        JArray arr = JsonHelper.GetArray(obj, currentKey);
                        res = JsonHelper.FindToken(arr[expression.Index], expression.SubExpression.Key, currentKey);
                        if (res == null)
                        {
                            arr = FillArrayToCount(arr, expression.Index ?? 0);
                            if (arr[expression.Index] == null)
                            {
                                arr[expression.Index] = CreateObject(new JObject(), expression.SubExpression);
                                res = JsonHelper.FindToken(arr[expression.Index], expression.SubExpression.Key, currentKey);
                            }
                        }
                        token = arr;
                    }
                    else
                    {
                        JObject o = JsonHelper.GetObject(obj, currentKey);
                        res = JsonHelper.FindToken(o, expression.SubExpression.Key, currentKey);
                        if (res == null)
                        {
                            o = CreateObject(o, expression.SubExpression);
                            res = JsonHelper.FindToken(o, expression.SubExpression.Key, currentKey);
                        }
                        token = o;
                    }

                    if (res?.Parent != null)
                    {
                        var prop = (JProperty)res.Parent;
                        var newValue = argument.Format(kvp.Value, true);
                        if (setting.AsString)
                        {
                            prop.Value = newValue;
                        }
                        else if (JsonHelper.IsArray(newValue))
                        {
                            JArray newArr = JsonHelper.GetArray(newValue, kvp.Value);
                            prop.Value = newArr;
                        }
                        else if (JsonHelper.IsObject(newValue))
                        {
                            JObject newObj = JsonHelper.GetObject(newValue, kvp.Value);
                            prop.Value = newObj;
                        }
                        else
                        {
                            prop.Value = newValue;
                        }
                    }
                    argument.PutPrivate(currentKey, JsonConvert.SerializeObject(token));
                    results.Add(currentKey, token);
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
        /// <summary>
        /// set value as string
        /// </summary>
        public bool AsString { get; set; }
    }

}
