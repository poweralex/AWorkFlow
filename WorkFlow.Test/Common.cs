using AWorkFlow2.Models;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WorkFlow.Test
{
    class Common
    {
        public static void ValidateOperationResult(OperationResult result)
        {
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
        }

        public static void ValidateOperationResultWithData<T>(OperationResult<T> result)
        {
            ValidateOperationResult(result);
            Assert.IsNotNull(result.Data);
        }

        public static void CompareObject<T>(T o1, T o2, params string[] expectProperties)
        {
            var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (expectProperties?.Any(x => x == property.Name) == true)
                {
                    continue;
                }
                var v1 = property.GetValue(o1);
                var v2 = property.GetValue(o2);
                if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                {
                    Assert.AreEqual(v1, v2);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    CompareList((IEnumerable<object>)v1, (IEnumerable<object>)v2);
                }
                else
                {
                    Assert.AreEqual(v1, v2);
                }
            }
        }

        public static void CompareList<T>(IEnumerable<T> list1, IEnumerable<T> list2, Func<T, dynamic> keySelector = null)
        {
            Assert.IsTrue((list1 == null && list2 == null) || (list1 != null && list2 != null));
            if (list1 == null)
            {
                return;
            }
            Assert.IsNotNull(list1);
            Assert.IsNotNull(list2);
            Assert.AreEqual(list1.Count(), list2.Count());
            if (keySelector == null)
            {
                keySelector = x => x.GetHashCode();
            }
            foreach (var o1 in list1)
            {
                var key1 = keySelector(o1);
                var o2 = list2.FirstOrDefault(x => keySelector(x) == key1);
                CompareObject(o1, o2);
            }
        }

        public static void CompareTimeApproximate(DateTime? dt1, DateTime? dt2)
        {
            if (dt1 == null && dt2 == null)
            {
                return;
            }
            else
            {
                Assert.IsNotNull(dt1);
                Assert.IsNotNull(dt2);
                if (Math.Abs((dt2.Value - dt1.Value).TotalSeconds) > 2)
                {
                    Assert.Fail($"{dt1} <> {dt2}");
                }
            }
        }
    }
}
