//-----------------------------------------------------------------------
// <copyright file="ContentKeyTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class ContentKeyTest
    {
		private CloudMediaContext _mediaContext;
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public void SetupTest()
		{
			_mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
		}

		#region Retry Logic tests

		[TestMethod]
		[Priority(0)]
		public void ContentKeyBaseCollectionGetProtectionKeyIdForContentKeyRetry()
		{
			var dataContextMock = new Mock<IMediaDataServiceContext>();

			var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			var fakeResponse = new string[] { "testKey" };
			int exceptionCount = 2;

			dataContextMock.Setup((ctxt) => ctxt
				.Execute<string>(It.IsAny<Uri>()))
				.Returns(() =>
				{
					if (--exceptionCount > 0) throw fakeException;
					return fakeResponse;
				});

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			var actual = ContentKeyBaseCollection.GetProtectionKeyIdForContentKey(_mediaContext, ContentKeyType.CommonEncryption);

			Assert.AreEqual(fakeResponse[0], actual);

			dataContextMock.Verify((ctxt) => ctxt.Execute<string>(It.IsAny<Uri>()), Times.Exactly(2));
		}

		[TestMethod]
		[DeploymentItem(@"UnitTest.pfx")]
		public void ContentKeyGetEncryptedKeyValueRetry()
		{
			var data = new ContentKeyData { Name = "testData" };

			var dataContextMock = new Mock<IMediaDataServiceContext>();

			var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			string testKey = "testKey";
			var fakeResponse = new string[] { Convert.ToBase64String(new System.Text.UTF8Encoding().GetBytes(testKey)) };
			int exceptionCount = 2;

			dataContextMock.Setup((ctxt) => ctxt
				.Execute<string>(It.IsAny<Uri>()))
				.Returns(() =>
				{
					if (--exceptionCount > 0) throw fakeException;
					return fakeResponse;
				});

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);
			data.SetMediaContext(_mediaContext);

			var cert = new X509Certificate2("UnitTest.pfx");
			var actual = data.GetEncryptedKeyValue(cert);

			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Length > 0);

			dataContextMock.Verify((ctxt) => ctxt.Execute<string>(It.IsAny<Uri>()), Times.Exactly(2));
		}

		[TestMethod]
		[DeploymentItem(@"UnitTest.pfx")]
		public void ContentKeyGetClearKeyValueRetry()
		{
			var data = new ContentKeyData { Name = "testData" };

			var dataContextMock = new Mock<IMediaDataServiceContext>();

			var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			string testKey = "testKey";
			var fakeResponse = new string[] { Convert.ToBase64String(new System.Text.UTF8Encoding().GetBytes(testKey)) };
			int exceptionCount = 2;

			dataContextMock.Setup((ctxt) => ctxt
				.Execute<string>(It.IsAny<Uri>()))
				.Returns(() =>
				{
					if (--exceptionCount > 0) throw fakeException;
					return fakeResponse;
				});

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);
			data.SetMediaContext(_mediaContext);

			var cert = new X509Certificate2("UnitTest.pfx");
			var actual = data.GetClearKeyValue();

			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Length > 0);

			dataContextMock.Verify((ctxt) => ctxt.Execute<string>(It.IsAny<Uri>()), Times.Exactly(2));
		}

		[TestMethod]
		public void ContentKeyUpdateRetry()
		{
			var data = new ContentKeyData { Name = "testData" };
			var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
			var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

			dataContextMock.Setup((ctxt) => ctxt.AttachTo("Assets", data));
			dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			data.SetMediaContext(_mediaContext);

			data.Update();

			dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
		}

		[TestMethod]
		public void ContentKeyDeleteRetry()
		{
			var data = new ContentKeyData { Name = "testData" };

			var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

			dataContextMock.Setup((ctxt) => ctxt.AttachTo("Assets", data));
			dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			data.SetMediaContext(_mediaContext);

			data.Delete();

			dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
		}
		#endregion Retry Logic tests
    }
}
