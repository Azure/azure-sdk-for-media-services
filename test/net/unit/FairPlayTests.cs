//-----------------------------------------------------------------------
// <copyright file="FairPlayTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.FairPlay;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class FairPlayTests
    {
        [TestMethod]
        [DeploymentItem(@"UnitTest.pfx")]
        public void TestFairPlayConfigurationSerialization()
        {
            var cert = new X509Certificate2("UnitTest.pfx", "", X509KeyStorageFlags.Exportable);
            string password = "";
            var passwordId = Guid.NewGuid();
            var askId = Guid.NewGuid();
            var iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            string configuration = FairPlayConfiguration.CreateSerializedFairPlayOptionConfiguration(
                cert,
                password,
                passwordId,
                askId,
                iv,
                RentalAndLeaseKeyType.PersistentUnlimited,
                123);

            var result = JsonConvert.DeserializeObject<FairPlayConfiguration>(configuration);

            Assert.AreEqual(passwordId, result.FairPlayPfxPasswordId);
            Assert.AreEqual(askId, result.ASkId);
            Assert.AreEqual("0102030405060708090A0B0C0D0E0F10", result.ContentEncryptionIV);
            Assert.AreEqual(RentalAndLeaseKeyType.PersistentUnlimited, result.RentalAndLeaseKeyType);
            Assert.AreEqual(123U, result.RentalDuration);

            var cert2 = new X509Certificate2(Convert.FromBase64String(result.FairPlayPfx));
            Assert.AreEqual(cert.Thumbprint, cert2.Thumbprint);
        }
	}
}