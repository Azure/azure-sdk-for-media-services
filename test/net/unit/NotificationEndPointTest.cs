//-----------------------------------------------------------------------
// <copyright file="AssetUnitTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class NotificationEndPointTest
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void QueryNotificationsEndPoint()
        {
            Assert.IsNull(_mediaContext.NotificationEndPoints.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        [TestMethod]
        public void NotificationEndPointCRUD()
        {
            var endPoint = _mediaContext.NotificationEndPoints.Create(Guid.NewGuid().ToString(), NotificationEndPointType.AzureQueue, "http://Contoso.com");
           Assert.IsNotNull(endPoint);
           Assert.IsNotNull(endPoint.Id);
           Assert.IsFalse(String.IsNullOrEmpty(endPoint.Name));
           endPoint.Name = Guid.NewGuid().ToString();
           endPoint.Update();
           endPoint.Name = Guid.NewGuid().ToString();
           endPoint.UpdateAsync();
           endPoint.Delete();
           Assert.IsNull(_mediaContext.NotificationEndPoints.Where(c=>c.Id == endPoint.Id).FirstOrDefault());
           endPoint = _mediaContext.NotificationEndPoints.CreateAsync(Guid.NewGuid().ToString(), NotificationEndPointType.AzureQueue, "http://Contoso.com").Result;
           endPoint.DeleteAsync().Wait();
           Assert.IsNull(_mediaContext.NotificationEndPoints.Where(c => c.Id == endPoint.Id).FirstOrDefault());
        }

        [TestMethod]
        public void WebHookNotificationEndPointCRUD()
        {
            const string str = "12345678";
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);

            var endPoint = _mediaContext.NotificationEndPoints.Create(Guid.NewGuid().ToString(), NotificationEndPointType.WebHook, "http://Contoso.com", bytes);
            Assert.IsNotNull(endPoint);
            Assert.IsNotNull(endPoint.Id);
            Assert.IsFalse(String.IsNullOrEmpty(endPoint.Name));
            endPoint.Name = Guid.NewGuid().ToString();
            endPoint.Update();
            endPoint.Name = Guid.NewGuid().ToString();
            endPoint.UpdateAsync();
            endPoint.Delete();
            Assert.IsNull(_mediaContext.NotificationEndPoints.Where(c => c.Id == endPoint.Id).FirstOrDefault());
        }

        [TestMethod]
        public void NotificationEndPointCreationFailureForAzureQueueWithCredential()
        {
            const string str = "12345678";
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);

            try
            {
                var endPoint = _mediaContext.NotificationEndPoints.Create(Guid.NewGuid().ToString(), NotificationEndPointType.AzureQueue, "http://Contoso.com", bytes);
                Assert.Fail();
            }
            catch (NotSupportedException ex)
            {
                Assert.IsTrue(ex.Message.Contains(StringTable.SupportWebHookWithCredentialOnly));
            }
        }

        [TestMethod]
        public void NotificationEndPointCreateValidateParameters()
        {
            bool failed = false;
            try
            {
                var endPoint = _mediaContext.NotificationEndPoints.Create(null, NotificationEndPointType.AzureQueue, "http://Contoso.com");
            }
            catch (ArgumentNullException)
            {
                failed = true;
            }
            Assert.IsTrue(failed,"Expecting ArgumentNullException when endpoint name is null");

            failed = false;
            try
            {
                var endPoint = _mediaContext.NotificationEndPoints.Create(String.Empty, NotificationEndPointType.AzureQueue, "http://Contoso.com");
            }
            catch (ArgumentException)
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Expecting ArgumentException when endpoint name is empty");

            failed = false;
            try
            {
                var endPoint = _mediaContext.NotificationEndPoints.Create(Guid.NewGuid().ToString(), NotificationEndPointType.AzureQueue, null);
            }
            catch (ArgumentException)
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Expecting ArgumentException when endpoint address is null");
            failed = false;
            try
            {
                var endPoint = _mediaContext.NotificationEndPoints.Create(Guid.NewGuid().ToString(), NotificationEndPointType.AzureQueue, String.Empty);
            }
            catch (ArgumentException)
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Expecting ArgumentException when endpoint address is null");


        }

    }
}