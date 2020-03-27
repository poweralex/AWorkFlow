using AWorkFlow2.Models.Working;
using AWorkFlow2.Providers;
using AWorkFlow2.Providers.ActionExcutor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace WorkFlow.Test
{
    class ActionTests
    {
        [Test]
        public void TestGroupListProcessor()
        {
            var processor = new GroupListProcessor();
            var setting = new GroupListActionSetting
            {
                Source = "{{data.items}}",
                Key = "{{loopItem.type}}",
                Output = new Dictionary<string, string>
                {
                    { "key","{{groupKey}}"},
                    { "items","{{groupItems}}"}
                }
            };
            var data = new
            {
                key = "data-key",
                items = new List<dynamic>
                {
                    new { id = "1", type = "a"},
                    new { id = "2", type = "a"},
                    new { id = "3", type = "b"},
                    new { id = "4", type = "b"},
                }
            };
            var workingArgument = new WorkingArguments(publicArguments:
                new Dictionary<string, string>
                {
                    { "data", JsonConvert.SerializeObject(data)}
                });
            var argumentProvider = new ArgumentProvider(workingArgument);
            var result = processor.Execute(JsonConvert.SerializeObject(setting), argumentProvider).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.Fail);
            Assert.IsTrue(string.IsNullOrEmpty(result.Message));
            Assert.IsFalse(string.IsNullOrEmpty(result.Data));
            Assert.AreEqual(1, result.Output.Count);
            Assert.IsTrue(result.Output.ContainsKey("result"));
            var resultObj = JsonConvert.DeserializeObject<JArray>(result.Output["result"]);
            Assert.IsNotNull(resultObj);
            Assert.AreEqual(2, resultObj.Count);
            Assert.IsNotNull(resultObj[0]);
            var obj1 = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(resultObj[0]));
            Assert.IsNotNull(obj1);
            Assert.IsNotNull(obj1.key);
            Assert.IsNotNull(obj1.items);
            Assert.AreEqual("a", obj1.key.ToString());
            List<dynamic> items1 = JsonConvert.DeserializeObject<List<dynamic>>(obj1.items.ToString());
            Assert.AreEqual(2, items1.Count);
            Assert.IsTrue(items1?.Any(x => x.id == "1"));
            Assert.IsTrue(items1?.Any(x => x.id == "2"));

            Assert.IsNotNull(resultObj[1]);
            var obj2 = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(resultObj[1]));
            Assert.IsNotNull(obj2);
            Assert.IsNotNull(obj2.key);
            Assert.IsNotNull(obj2.items);
            Assert.AreEqual("b", obj2.key.ToString());
            List<dynamic> items2 = JsonConvert.DeserializeObject<List<dynamic>>(obj2.items.ToString());
            Assert.AreEqual(2, items2.Count);
            Assert.IsTrue(items2?.Any(x => x.id == "3"));
            Assert.IsTrue(items2?.Any(x => x.id == "4"));
        }
    }
}
