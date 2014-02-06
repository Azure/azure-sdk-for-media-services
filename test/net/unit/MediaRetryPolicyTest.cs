//-----------------------------------------------------------------------
// <copyright file="MediaRetryPolicyTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
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
            int expected = 10;
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
            int expected = 10;
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
            int expected = 10;
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
            int expected = 10;
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
            int expected = 10;
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
            int expected = 10;
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
            int expected = 10;
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
            int expected = 10;
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
