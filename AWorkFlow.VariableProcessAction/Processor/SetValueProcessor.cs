using AWorkFlow.Core.Extensions;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow.VariableProcessAction.Processor
{
    /// <summary>
    /// provide processor for type=VariableProcess and method=Set
    /// </summary>
    public class SetValueProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static string Method = "Set";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ExecutionResultDto> Execute(VariableProcessActionSetting setting, IExpressionProvider expressionProvider)
        {
            var results = new Dictionary<string, string>();
            foreach (var kvp in setting.Output)
            {
                var target = expressionProvider.Format(kvp.Key).Result;
                ExpressionDto expression = new ExpressionDto(string.Empty, target);
                var currentKey = expression.CurrentKey;
                if (expression.IsArray)
                {
                    currentKey = expression.ArrayKey;
                }
                var obj = expressionProvider.Arguments.Get(currentKey);
                if (obj == null)
                {
                    expressionProvider.Arguments.PutPrivate(target, expressionProvider.Format(kvp.Value).Result);
                    results.Add(target, expressionProvider.Arguments.Get(target));
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

                    var prop = (JProperty)res.Parent;
                    var newValue = expressionProvider.Format(kvp.Value).Result;
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
                    expressionProvider.Arguments.PutPrivate(currentKey, token.ToJson());
                    results.Add(currentKey, expressionProvider.Arguments.Get(expression.CurrentKey));
                }
            }

            return Task.FromResult(new ExecutionResultDto
            {
                Success = true,
                ExecuteResult = JsonConvert.SerializeObject(results)
            });
        }

        private JObject CreateObject(JObject obj, ExpressionDto expression)
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
}
