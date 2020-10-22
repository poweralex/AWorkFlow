using AWorkFlow2.Helps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// working arguments model
    /// </summary>
    public class WorkingArguments : WorkingModelBase
    {
        private readonly object lockObj = new object();
        /// <summary>
        /// working copy id
        /// </summary>
        public string WorkingCopyId { get; set; } = string.Empty;
        /// <summary>
        /// working step id
        /// </summary>
        public string WorkingStepId { get; set; } = string.Empty;
        /// <summary>
        /// working step result id
        /// </summary>
        public string WorkingStepResultId { get; set; } = string.Empty;
        /// <summary>
        /// action type
        /// </summary>
        public string ActionType { get; set; } = string.Empty;
        /// <summary>
        /// public arguments(which should be pass to next)
        /// </summary>
        public Dictionary<string, string> PublicArguments { get; private set; } = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        /// <summary>
        /// private arguments(which should NOT be pass to next)
        /// </summary>
        public Dictionary<string, string> PrivateArguments { get; private set; } = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="publicArguments"></param>
        /// <param name="privateArguments"></param>
        public WorkingArguments(Dictionary<string, string> publicArguments = null, Dictionary<string, string> privateArguments = null)
        {
            if (publicArguments != null)
            {
                PublicArguments = new Dictionary<string, string>(publicArguments, StringComparer.CurrentCultureIgnoreCase);
            }
            if (privateArguments != null)
            {
                PrivateArguments = new Dictionary<string, string>(privateArguments, StringComparer.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// set id(s) and action type
        /// </summary>
        /// <param name="workId"></param>
        /// <param name="stepId"></param>
        /// <param name="resultId"></param>
        /// <param name="actionType"></param>
        public void SetIds(string workId, string stepId, string resultId, string actionType)
        {
            lock (lockObj)
            {
                WorkingCopyId = workId;
                WorkingStepId = stepId;
                WorkingStepResultId = resultId;
                ActionType = actionType;
                if (PrivateArguments != null)
                {
                    PrivateArguments["workingCopyId"] = workId;
                    PrivateArguments["workingStepId"] = stepId;
                }
            }
        }

        /// <summary>
        /// copy to a new instance with new dictionaries
        /// </summary>
        /// <returns></returns>
        public WorkingArguments Copy()
        {
            return new WorkingArguments(new Dictionary<string, string>(PublicArguments), new Dictionary<string, string>(PrivateArguments))
            {
                WorkingCopyId = WorkingCopyId,
                WorkingStepId = WorkingStepId,
                WorkingStepResultId = WorkingStepResultId,
                ActionType = ActionType
            };
        }

        /// <summary>
        /// expand "output" in public arguments
        /// </summary>
        /// <returns></returns>
        public WorkingArguments ExpandOutputs()
        {
            // process output
            var outputKeys = PublicArguments.Where(x => "output".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase))?.Select(x => x.Key)?.ToList();
            if (outputKeys?.Any() == true)
            {
                foreach (var outputKey in outputKeys)
                {
                    if (PublicArguments.Remove(outputKey, out string outputValue))
                    {
                        var output = JsonConvert.DeserializeObject<Dictionary<string, string>>(outputValue);
                        foreach (var kvp in output)
                        {
                            PublicArguments[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// copy "output" and expand into public arguments
        /// </summary>
        /// <param name="fromDic"></param>
        /// <returns></returns>
        public WorkingArguments CopyOutputs(Dictionary<string, string> fromDic)
        {
            if (fromDic == null)
            {
                return this;
            }
            // process output
            var outputKeys = fromDic.Where(x => "output".Equals(x.Key, StringComparison.CurrentCultureIgnoreCase))?.Select(x => x.Key)?.ToList();
            if (outputKeys?.Any() == true)
            {
                foreach (var outputKey in outputKeys)
                {
                    string outputValue = fromDic[outputKey];
                    if (!string.IsNullOrEmpty(outputValue))
                    {
                        var output = JsonConvert.DeserializeObject<Dictionary<string, string>>(outputValue);
                        foreach (var kvp in output)
                        {
                            PublicArguments[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// clear specific key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onPublic"></param>
        /// <param name="onPrivate"></param>
        public void ClearKey(string key, bool onPublic = true, bool onPrivate = true)
        {
            if (onPublic && PublicArguments?.ContainsKey(key) == true)
            {
                PublicArguments.Remove(key);
            }
            if (onPrivate && PrivateArguments?.ContainsKey(key) == true)
            {
                PrivateArguments.Remove(key);
            }
        }

        /// <summary>
        /// remove except keys
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="onPublic"></param>
        /// <param name="onPrivate"></param>
        public WorkingArguments FilterKeys(IEnumerable<string> keys, bool onPublic = true, bool onPrivate = true)
        {
            if (keys?.Any() != true)
            {
                return this;
            }
            if (onPublic)
            {
                FilterKeys(PublicArguments, keys);
            }
            if (onPrivate)
            {
                FilterKeys(PrivateArguments, keys);
            }

            return this;
        }

        private void FilterKeys(Dictionary<string, string> dic, IEnumerable<string> exceptKeys)
        {
            if (dic == null || exceptKeys?.Any() != true)
            {
                return;
            }
            var toRemoveKeys = dic.Keys.Except(exceptKeys).ToList();
            foreach (var key in toRemoveKeys)
            {
                dic.Remove(key);
            }
        }

        /// <summary>
        /// accept all changes include sub-items
        /// </summary>
        /// <param name="acceptAll"></param>
        public void AcceptChanges(bool acceptAll)
        {
            base.AcceptChanges();
        }

        #region Merge
        /// <summary>
        /// merge arguments
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="intoList"></param>
        /// <returns></returns>
        public static WorkingArguments Merge(IEnumerable<WorkingArguments> arguments, bool intoList)
        {
            WorkingArguments result = new WorkingArguments();
            if (arguments?.Any() == true)
            {
                // merge ids
                if (arguments?.Select(x => x.WorkingCopyId)?.Distinct()?.Count() == 1)
                {
                    result.WorkingCopyId = arguments?.First()?.WorkingCopyId;
                }
                if (arguments?.Select(x => x.WorkingStepId)?.Distinct()?.Count() == 1)
                {
                    result.WorkingStepId = arguments?.First()?.WorkingStepId;
                }
                if (arguments?.Select(x => x.WorkingStepResultId)?.Distinct()?.Count() == 1)
                {
                    result.WorkingStepResultId = arguments?.First()?.WorkingStepResultId;
                }
                if (arguments?.Select(x => x.ActionType)?.Distinct()?.Count() == 1)
                {
                    result.ActionType = arguments?.First()?.ActionType;
                }

                result.PublicArguments = MergeDictionary(arguments?.Select(x => x.PublicArguments), intoList);
                result.PrivateArguments = MergeDictionary(arguments?.Select(x => x.PrivateArguments), intoList);
            }

            return result;
        }

        private static Dictionary<string, string> MergeDictionary(IEnumerable<Dictionary<string, string>> dics, bool intoList)
        {
            Dictionary<string, List<string>> tmpDic = new Dictionary<string, List<string>>();
            if (dics?.Any() != true)
            {
                return new Dictionary<string, string>();
            }

            foreach (var dic in dics)
            {
                if (dic?.Any() == true)
                {
                    foreach (var kvp in dic)
                    {
                        if (!tmpDic.ContainsKey(kvp.Key))
                        {
                            tmpDic[kvp.Key] = new List<string>();
                        }
                        tmpDic[kvp.Key].Add(kvp.Value);
                    }
                }
            }

            // merge dics
            if (tmpDic?.Any() != true)
            {
                return new Dictionary<string, string>();
            }

            if (intoList)
            {
                return tmpDic.ToDictionary(kvp => kvp.Key, kvp =>
                {
                    if (!kvp.Value.Any() || (JsonHelper.IsObject(kvp.Value[0]) || JsonHelper.IsArray(kvp.Value[0])))
                    {
                        return $"[{string.Join(",", kvp.Value)}]";
                    }
                    else
                    {
                        return $"[{string.Join(",", kvp.Value.Select(x => $"\"{x}\""))}]";
                    }
                });
            }
            else
            {
                return tmpDic.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FirstOrDefault());
            }
        }
        #endregion
    }
}
