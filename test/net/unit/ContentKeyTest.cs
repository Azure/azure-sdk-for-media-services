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
using System.Collections.Generic;
using System.Linq;
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
        private readonly byte[] ContentKeyBytes = new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

       
        [TestMethod]
        public void ContentKeyCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes);
            Assert.IsNotNull(_mediaContext.ContentKeys.Where(c=>c.Id == key.Id).FirstOrDefault());
            Assert.AreEqual(ContentKeyType.CommonEncryption, key.ContentKeyType);
            Assert.AreEqual(ProtectionKeyType.X509CertificateThumbprint, key.ProtectionKeyType);
            UpdateDeleteContentKey(key);
            key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes);
            key.DeleteAsync();
        }
        [TestMethod]
        public void ContentKeyCommonEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.CommonEncryption);
            UpdateDeleteContentKey(key);

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContentKeyConfigurationEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.ConfigurationEncryption);
            UpdateDeleteContentKey(key);

        }
        [TestMethod]
        public void ContentKeyEnvelopeEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.EnvelopeEncryption);
            UpdateDeleteContentKey(key);

        }
        [TestMethod]
        public void ContentKeyTrackIdentifiersCRUD()
        {
            var tracks = new List<string> {"mp4a", "aacl"};
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes, Guid.NewGuid().ToString(), ContentKeyType.EnvelopeEncryption, tracks);
            var check = _mediaContext.ContentKeys.Single(k => k.Id.Equals(key.Id, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("mp4a,aacl", check.TrackIdentifiers);
            UpdateDeleteContentKey(key);
        }

        [TestMethod]
        public void ContentKeyStorageEncryptionEncryptionCRUD()
        {
            byte[] keyData = Enumerable.Range(0, 32).Select(i => (byte)1).ToArray();
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), keyData, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.StorageEncryption);
            UpdateDeleteContentKey(key);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContentKeyUrlEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.UrlEncryption);
            UpdateDeleteContentKey(key);

        }

        private static void UpdateDeleteContentKey(IContentKey key)
        {
            key.AuthorizationPolicyId = Guid.NewGuid().ToString();
            key.Update();
            key.AuthorizationPolicyId = Guid.NewGuid().ToString();
            key.UpdateAsync();
            key.Delete();
        }


        [TestMethod]
        public void LinkContentKeyToAsset()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes);
            IAsset asset = _mediaContext.Assets.Create("LinkContentKeyToAsset", AssetCreationOptions.StorageEncrypted);
            asset.ContentKeys.Add(key);
            var keys = asset.ContentKeys.ToList();
            Assert.AreEqual(2, keys.Count);
            asset.ContentKeys.Remove(key);
            Assert.AreEqual(1, asset.ContentKeys.Count);

        }
        [TestMethod]
        public void CreateShortContentKeyAsyncWithEmptyNameShouldPass()
        {
            var key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyBytes, String.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateShortContentKeyShouldFail()
        {
            var key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[1] { 1 });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateContentKeyWithEmptyIdShouldFail()
        {
            var key = _mediaContext.ContentKeys.CreateAsync(Guid.Empty, new byte[1] { 1 }).Result;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateContentKeyWithEmptyBodyShouldFail()
        {
            var key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), null);
            Assert.IsNotNull(_mediaContext.ContentKeys.Where(c => c.Id == key.Id).FirstOrDefault());
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
