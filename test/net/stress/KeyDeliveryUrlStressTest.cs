//-----------------------------------------------------------------------
// <copyright file="KeyDeliveryUrlStressTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Stress
{
    [TestClass]
    public class KeyDeliveryUrlStressTest
    {
        private static CloudMediaContext _mediaContext;
        private static readonly List<Tuple<Uri, string, string>> _testData = new List<Tuple<Uri, string, string>>();
        private Random rnd = new Random();
        private static PerformanceCounter numberOfOperationsPerformanceCounter;
        private static PerformanceCounter operationsPerSecondCounter;

        //Initialization code which runs once per test run

        /// <summary>
        /// Prepoluate content keys to be used in stress test
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
         CreateCounters();
            
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IContentKeyAuthorizationPolicyOption policyOption = null;

            for (int i = 0; i < 10; i++)
            {
                byte[] expectedKey = null;
                IContentKey contentKey = GetKeyDeliveryUrlTests.CreateTestKey(_mediaContext, ContentKeyType.EnvelopeEncryption, out expectedKey);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.BaselineHttp, null, null, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                GetKeyDeliveryUrlTests.CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);

                Assert.IsNotNull(keyDeliveryServiceUri);
                string rawkey = EncryptionUtils.GetKeyIdAsGuid(contentKey.Id).ToString();
                _testData.Add(new Tuple<Uri, string, string>(keyDeliveryServiceUri, TokenServiceClient.GetAuthTokenForKey(rawkey), GetKeyDeliveryUrlTests.GetString(expectedKey)));
            }
            
        }

        [TestInitialize]
        public void SetupTest()
        {
           
        }

        [TestMethod]
        public void KeyDeliveryUrlStressSample()
        {
            for (int i = 0; i < 50; i++)
            {
                var current = _testData[rnd.Next(_testData.Count)];
                KeyDeliveryServiceClient keyClient = new KeyDeliveryServiceClient(RetryPolicy.DefaultFixed);
                var key = GetKeyDeliveryUrlTests.GetString(keyClient.AcquireHlsKey(current.Item1, current.Item2));
                Assert.AreEqual(current.Item3, key);
                numberOfOperationsPerformanceCounter.Increment();
                operationsPerSecondCounter.Increment();
            }
        }

        private static void CreateCounters()
        {
            // Create the counters.
            numberOfOperationsPerformanceCounter = new PerformanceCounter("AMSStressCounterSet", "# operations executed", false);
            operationsPerSecondCounter = new PerformanceCounter("AMSStressCounterSet", "# operations / sec", false);
            numberOfOperationsPerformanceCounter.RawValue = 0;
            operationsPerSecondCounter.RawValue = 0;
        }
    }
}


