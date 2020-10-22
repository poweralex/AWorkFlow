using AWorkFlow2.Models.Working;
using NUnit.Framework;
using System;

namespace WorkFlow.Test
{
    class TrackingChangeTests
    {
        [Test]
        public void TestInsertNoChange()
        {
            var work = new WorkingCopy();
            Assert.IsTrue(work.Inserted);
            Assert.IsFalse(work.Updated);
        }

        [Test]
        public void TestInsertAndChange()
        {
            var work = new WorkingCopy();
            Assert.IsTrue(work.Inserted);
            Assert.IsFalse(work.Updated);
            work.BeginTime = DateTime.Now;
            Assert.IsTrue(work.Inserted);
            Assert.IsFalse(work.Updated);
        }

        [Test]
        public void TestAcceptNoChange()
        {
            var work = new WorkingCopy();
            work.AcceptChanges();
            Assert.IsFalse(work.Inserted);
            Assert.IsFalse(work.Updated);
        }

        [Test]
        public void TestChange()
        {
            var work = new WorkingCopy();
            work.AcceptChanges();
            work.BeginTime = DateTime.Now;
            Assert.IsFalse(work.Inserted);
            Assert.IsTrue(work.Updated);
        }

        [Test]
        public void TestChangeIgnore()
        {
            var work = new WorkingCopy();
            work.AcceptChanges();
            work.Arguments = new WorkingArguments();
            Assert.IsFalse(work.Inserted);
            Assert.IsFalse(work.Updated);
        }

        [Test]
        public void TestChangeArguments()
        {
            var arguments = new WorkingArguments();
            Assert.IsTrue(arguments.Inserted);
            Assert.IsFalse(arguments.Updated);
            arguments.AcceptChanges();
            arguments.PrivateArguments.Add("now", $"{DateTime.UtcNow}");
            Assert.IsFalse(arguments.Inserted);
            Assert.IsTrue(arguments.Updated);

        }
    }
}
