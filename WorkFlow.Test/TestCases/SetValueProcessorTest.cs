using AWorkFlow2.Models.Working;
using AWorkFlow2.Providers;
using AWorkFlow2.Providers.ActionExcutor;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;

namespace WorkFlow.Test
{
    class SetValueProcessorTest
    {
        [Test]
        public void TestEventBody()
        {
            WorkingArguments workingArguments = new WorkingArguments();
            ArgumentProvider argProvider = new ArgumentProvider(workingArguments);
            argProvider.PutPrivate("lastCheckResult", "{\"data\":null,\"success\":false,\"code\":null,\"message\":\"Cannot get any products by style number: [20200122].\",\"exception\":null}");
            argProvider.PutPrivate("now", "2019/12/11T09:00:00.000Z");
            argProvider.PutPrivate("input.merchantOrderId", "merchantOrderId");
            var targetStr = "{\"message\":\"{\\\"name\\\":\\\"InvalidOrder\\\",\\\"time\\\":\\\"2019/12/11T09:00:00.000Z\\\",\\\"data\\\":{\\\"orderId\\\":\\\"merchantOrderId\\\",\\\"messages\\\":{\\\"data\\\":null,\\\"success\\\":false,\\\"code\\\":null,\\\"message\\\":\\\"Cannot get any products by style number: [20200122].\\\",\\\"exception\\\":null}}}\"}";

            var setProcessor = new SetValueProcessor();
            var setSetting1 = new SetValueActionSetting
            {
                Set = new Dictionary<string, string> {
                { "orderId", "{{input.merchantOrderId}}" },
                    { "messages","{{lastCheckResult}}"}
                }
            };
            var res1 = setProcessor.Execute(JsonConvert.SerializeObject(setSetting1), argProvider).Result;
            argProvider.PutPrivate("event_msg_data", res1.Data);

            var setSetting2 = new SetValueActionSetting
            {
                Set = new Dictionary<string, string> {
                { "name", "InvalidOrder" },
                    { "time","{{now}}"},
                    { "data","{{event_msg_data}}"}
                }
            };
            var res2 = setProcessor.Execute(JsonConvert.SerializeObject(setSetting2), argProvider).Result;
            argProvider.PutPrivate("event_msg", res2.Data);

            var setSetting3 = new SetValueActionSetting
            {
                Set = new Dictionary<string, string> {
                { "message", "{{event_msg}}" }
                },
                AsString = true
            };
            var res3 = setProcessor.Execute(JsonConvert.SerializeObject(setSetting3), argProvider).Result;
            argProvider.PutPrivate("event_body", res3.Data);


            var resultStr = argProvider.Format("{{event_body}}");
            Assert.AreEqual(targetStr, resultStr);
        }

        [Test]
        public void TestSetValueOfObject()
        {
            WorkingArguments workingArguments = new WorkingArguments();
            ArgumentProvider argProvider = new ArgumentProvider(workingArguments);
            argProvider.PutPrivate("lastCheckResult", "{\"data\":null,\"success\":false,\"code\":null,\"message\":\"Cannot get any products by style number: [20200122].\",\"exception\":null}");
            argProvider.PutPrivate("data", "{\"data\":null}");
            var targetStr = "{\"lastCheckResult\":\"{\\\"data\\\":{\\\"data\\\":null},\\\"success\\\":false,\\\"code\\\":null,\\\"message\\\":\\\"Cannot get any products by style number: [20200122].\\\",\\\"exception\\\":null}\"}";

            var processor = new SetValueProcessor();
            var setting = new SetValueActionSetting
            {
                Set = new Dictionary<string, string>
                {
                    { "lastCheckResult.data","{{data}}"}
                }
            };
            var res = processor.Execute(JsonConvert.SerializeObject(setting), argProvider).Result;

            Assert.AreEqual(targetStr, res.Data);
        }
    }
}
