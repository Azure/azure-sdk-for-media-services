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
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    /// <summary>
    ///This is a test class for CustomMediaRetryPolicyTest and is intended
    ///to contain all CustomMediaRetryPolicyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CustomMediaRetryPolicyTest
    {
        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void CustomMediaRetryPolicyTestExecuteActionTrivial()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactoryForCustomRetryPolicy(null).GetSaveChangesRetryPolicy(null);
            int expected = 10;
            Func<int> func = () => expected;
            int actual = target.ExecuteAction(func);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for validating that customretrypolicy is invoked
        ///</summary>
        [TestMethod()]
        public void CustomMediaRetryPolicyTestExecuteActionRetry()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactoryForCustomRetryPolicy(null).GetSaveChangesRetryPolicy(null);

            int exceptionCount = 3;
            int expected = 10;
            //This is the new exception included for retrypolicy in the customretrypolicy
            var fakeException = new IOException("CustomRetryPolicyException");

            Func<int> func = () =>
            {
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            int actual = target.ExecuteAction(func);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, exceptionCount);
        }

        /// <summary>
        ///A test for validating that customretrypolicy defined exceptions are not retried when using the default 
        ///TestMediaServicesClassFactory
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(IOException))]
        public void DefaultMediaRetryPolicyTestExecuteActionNonTransient()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactory(null).GetSaveChangesRetryPolicy(null);

            int exceptionCount = 2;
            int expected = 10;
            //IOException should not be retried when using default TestMediaServicesClassFactory instead of
            //CustomTestMediaServicesClassFactory
            var fakeException = new IOException("CustomRetryPolicyException");

            Func<int> func = () =>
            {
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            try
            {
                target.ExecuteAction(func);
            }
            catch (IOException x)
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
        public void CustomMediaRetryPolicyTestExecuteAsyncTrivial()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactoryForCustomRetryPolicy(null).GetSaveChangesRetryPolicy(null);
            int expected = 10;
            var task = target.ExecuteAsync(() => Task.Factory.StartNew<int>(() => expected));
            Assert.AreEqual(expected, task.Result);
        }

        /// <summary>
        ///A test for ExecuteAction
        ///</summary>
        [TestMethod()]
        public void CustomMediaRetryPolicyTestExecuteAsyncRetry()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactoryForCustomRetryPolicy(null).GetSaveChangesRetryPolicy(null);

            int exceptionCount = 2;
            int expected = 10;
            var fakeException = new IOException("Test CustomMediaRetryPolicyTestExecuteAsyncRetry");

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
        ///Validating that non transient failures are not retried when tried from the custom retry policy
        ///defined in CustomAzureMediaServicesClass
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CustomMediaRetryPolicyTestExecuteAsyncNonTransient()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactoryForCustomRetryPolicy(null).GetSaveChangesRetryPolicy(null);

            int exceptionCount = 2;
            int expected = 10;
            var fakeException = new InvalidOperationException("Test CustomMediaRetryPolicyTestExecuteAsyncRetry");

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
                InvalidOperationException x = (InvalidOperationException)ax.Flatten().InnerException;
                Assert.AreEqual(1, exceptionCount);
                Assert.AreEqual(fakeException, x);
                throw x;
            }

            Assert.Fail("Expected exception");
        }

        /// <summary>
        ///A test for validating that exception is not retried after max attempts defined in retry policy
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(IOException))]
        public void CustomMediaRetryPolicyTestExceededMaxRetryAttempts()
        {
            MediaRetryPolicy target = new TestMediaServicesClassFactoryForCustomRetryPolicy(null).GetBlobStorageClientRetryPolicy();

            int exceptionCount = 4;
            int expected = 10;
            //This is the new exception included for retrypolicy in the customretrypolicy
            var fakeException = new IOException("CustomRetryPolicyException");

            Func<int> func = () =>
            {
                if (--exceptionCount > 0) throw fakeException;
                return expected;
            };

            try
            {
                target.ExecuteAction(func);
            }
            catch (AggregateException ax)
            {
                IOException x = (IOException)ax.Flatten().InnerException;
                Assert.AreEqual(1, exceptionCount);
                Assert.AreEqual(fakeException, x);
                //Exception is retried only for max retrial attempts,
                //In this case there are max of 2 attempts for blob retry policy.
                Assert.AreEqual(exceptionCount, 1);
                throw;
            }

        }
    }

}
