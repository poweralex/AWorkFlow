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
            work.Steps = new System.Collections.Generic.List<WorkingCopyStep>();
            Assert.IsFalse(work.Inserted);
            Assert.IsFalse(work.Updated);
        }
    }
}
