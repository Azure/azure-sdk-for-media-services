//-----------------------------------------------------------------------
// <copyright file="Helper.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    public class Helper
    {
        public static CloudMediaContext GetMediaDataServiceContextForUnitTests(int delaymilliseconds = 0)
        {
            CloudMediaContext mediaContext = new TestCloudMediaContext(new Uri("http://contoso.com"), null);
            TestCloudMediaDataContext testCloudMediaDataContext = new TestCloudMediaDataContext(mediaContext,delaymilliseconds);
            mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(testCloudMediaDataContext);
            testCloudMediaDataContext.InitilizeStubData();
            return mediaContext;
        }

        public static MediaContextBase GetMockContextWithNullDefaultStorage()
        {
            var contextMock = new Mock<MediaContextBase>();
            contextMock.Setup(c => c.DefaultStorageAccount).Returns(() => { return null; });
            var context = contextMock.Object;
            Assert.IsNull(context.DefaultStorageAccount);
            return context;
        }
    }
}