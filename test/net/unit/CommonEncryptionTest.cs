//-----------------------------------------------------------------------
// <copyright file="CommonEncryptionTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit;
using Moq;

namespace UnitTests.ClassTests.common.EncryptionUnitTests
{
    [TestClass]
	public class CommonEncryptionTest
    {
		private CloudMediaContext _mediaContext;
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public void SetupTest()
		{
			_mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
		}

        [TestMethod]
		[DeploymentItem(@"UnitTest.pfx")]
        public void EncryptContentKeyToCertRoundTripTest()
        {
			var cert = new X509Certificate2("UnitTest.pfx");

			var dataContextMock = new Mock<IMediaDataServiceContext>();

			string testKey = "1234567890123456";
			var fakeResponse = new string[] { Convert.ToBase64String(new System.Text.UTF8Encoding().GetBytes(testKey)) };

			dataContextMock.Setup((ctxt) => ctxt
				.Execute<string>(It.IsAny<Uri>()))
				.Returns(() =>
				{
					return fakeResponse;
				});

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			var contentKey = new ContentKeyData { Name = "testData", Id = "id" };
			contentKey.SetMediaContext(_mediaContext);

			byte[] encryptedKeyValue = contentKey.GetEncryptedKeyValue(cert);

			byte[] encryptedContentKey = CommonEncryption.EncryptContentKeyToCertificate(cert, encryptedKeyValue);

            byte[] decryptedContentKey = EncryptionUtils.DecryptSymmetricKey(cert, encryptedContentKey);

			Assert.IsTrue(encryptedKeyValue.SequenceEqual(decryptedContentKey));
        }

        [TestMethod]
        public void PlayReadyContentKeyGenerationKnownGoodTest()
        {
			string _keySeed = "XVBovsmzhP9gRIZxWfFta3VVRPzVEWmJsazEJ46I";
			string _knownGoodGuid = "339C45B5-FB6D-4BD9-994C-EABD9D41C95B";
			string _wellKnownKeyString = "pTfNwMplhBQG1SXnvyLXVA==";

            byte[] keySeedInBinary = Convert.FromBase64String(_keySeed);
            Guid wellKnownGuid = new Guid(_knownGoodGuid);

            byte[] generatedKey = CommonEncryption.GeneratePlayReadyContentKey(keySeedInBinary, wellKnownGuid);

            string generatedKeyAsString = Convert.ToBase64String(generatedKey);

            Assert.AreEqual<string>(_wellKnownKeyString, generatedKeyAsString);
        }
    }
}
