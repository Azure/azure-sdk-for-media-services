using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    
    
    /// <summary>
    ///This is a test class for MediaRetryPolicyTest and is intended
    ///to contain all MediaRetryPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MediaRetryPolicyTest
    {
        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void MediaRetryPolicyTestExecuteActionTrivial()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();
            int expected = 0;
            Func<int> func = () => expected;
            int actual = target.ExecuteAction(func);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void MediaRetryPolicyTestExecuteActionRetry()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();

            int exceptionCount = 2;
            int expected = 0;
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            Func<int> func = () => {
                    if (--exceptionCount > 0) throw fakeException;
                    return expected;
                };

            int actual = target.ExecuteAction(func);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, exceptionCount);
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(WebException))]
        public void MediaRetryPolicyTestExecuteActionNonTransient()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();

            int exceptionCount = 2;
            int expected = 0;
            var fakeException = new WebException("test", WebExceptionStatus.RequestCanceled);

            Func<int> func = () =>
            {
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            try
            {
                target.ExecuteAction(func);
            }
            catch (WebException x)
            {
                Assert.AreEqual(1, exceptionCount);
                Assert.AreEqual(fakeException, x);
                throw;
            }

            Assert.Fail("Expected exception");
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void MediaRetryPolicyTestExecuteActionBackoff()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();

            int exceptionCount = 5;
            int expected = 0;
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            TimeSpan lastInterval = TimeSpan.Zero;
            DateTime lastInvoked = DateTime.UtcNow;

            Func<int> func = () =>
            {
                TimeSpan newInterval = DateTime.UtcNow - lastInvoked;
                TimeSpan delta = newInterval - lastInterval;
                Assert.IsTrue(exceptionCount > 3 || delta.TotalMilliseconds > 1, "Iterations left:{0} interval increase too small from {1} to {2}", exceptionCount, lastInterval, newInterval, delta);
                lastInvoked = DateTime.UtcNow;
                lastInterval = newInterval;
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            int actual = target.ExecuteAction(func);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, exceptionCount);
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void MediaRetryPolicyTestExecuteAsyncTrivial()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();
            int expected = 0;
            var task = target.ExecuteAsync(() => Task.Factory.StartNew<int>(() => expected));
            Assert.AreEqual(expected, task.Result);
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void MediaRetryPolicyTestExecuteAsyncRetry()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();

            int exceptionCount = 2;
            int expected = 0;
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            Func<int> func = () =>
            {
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            var task = target.ExecuteAsync(() => Task.Factory.StartNew<int>(() => func()));
            Assert.AreEqual(expected, task.Result);
            Assert.AreEqual(0, exceptionCount);
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(WebException))]
        public void MediaRetryPolicyTestExecuteAsyncNonTransient()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();

            int exceptionCount = 2;
            int expected = 0;
            var fakeException = new WebException("test", WebExceptionStatus.RequestCanceled);

            Func<int> func = () =>
            {
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            try
            {
                var task = target.ExecuteAsync(() => Task.Factory.StartNew<int>(() => func()));
                task.Wait();
                var result = task.Result;
            }
            catch (AggregateException ax)
            {
                WebException x = (WebException)ax.Flatten().InnerException;
                Assert.AreEqual(1, exceptionCount);
                Assert.AreEqual(fakeException, x);
                throw x;
            }

            Assert.Fail("Expected exception");
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void MediaRetryPolicyTestExecuteAsyncBackoff()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy();

            int exceptionCount = 5;
            int expected = 0;
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            TimeSpan lastInterval = TimeSpan.Zero;
            DateTime lastInvoked = DateTime.UtcNow;

            Func<int> func = () =>
            {
                TimeSpan newInterval = DateTime.UtcNow - lastInvoked;
                TimeSpan delta = newInterval - lastInterval;
                Assert.IsTrue(exceptionCount > 3 || delta.TotalMilliseconds > 1, "Iterations left:{0} interval increase too small from {1} to {2}", exceptionCount, lastInterval, newInterval, delta);
                lastInvoked = DateTime.UtcNow;
                lastInterval = newInterval;
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            var task = target.ExecuteAsync(() => Task.Factory.StartNew<int>(() => func()));
            Assert.AreEqual(expected, task.Result);
            Assert.AreEqual(0, exceptionCount);
        }
    }
}
