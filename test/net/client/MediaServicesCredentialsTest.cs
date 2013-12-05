//-----------------------------------------------------------------------
// <copyright file="MediaServicesCredentialsTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{    
    /// <summary>
    ///This is a test class for MediaServicesCredentialsTest and is intended
    ///to contain all MediaServicesCredentialsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MediaServicesCredentialsTest
    {
        [TestMethod()]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsTestReuseToken()
        {
            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            MediaServicesCredentials credentials = new MediaServicesCredentials("whatever", "whatever")
            {
                AccessToken = context1.Credentials.AccessToken,
                TokenExpiration = context1.Credentials.TokenExpiration
            };

            var context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext(credentials);
            context2.Assets.FirstOrDefault();
        }

        [TestMethod()]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsTestGetToken()
        {
            MediaServicesCredentials target = WindowsAzureMediaServicesTestConfiguration.CreateMediaServicesCredentials();

            Assert.IsNull(target.AccessToken);
            Assert.IsTrue(target.TokenExpiration < DateTime.UtcNow);

            target.RefreshToken();

            Assert.IsNotNull(target.AccessToken);
            Assert.IsTrue(target.AccessToken.Length > 0);
            Assert.IsTrue(target.TokenExpiration > DateTime.UtcNow.AddHours(1));
        }

        [TestMethod()]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsTestSetAcsToken()
        {
            MediaServicesCredentials target = new MediaServicesCredentials("dummyClientId", "dummyClientSecret", "dummyScope", "dummyAcsBaseAddress");

            string testAcsResponse = "{\"token_type\":\"http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0\",\"access_token\":\"SomeToken\",\"expires_in\":\"36000\",\"scope\":\"urn:Nimbus\"}";
            byte[] acsResponse = new System.Text.UTF8Encoding().GetBytes(testAcsResponse);
            target.SetAcsToken(acsResponse);

            Assert.AreEqual("SomeToken", target.AccessToken);

            var tokenExpiresIn = target.TokenExpiration - DateTime.UtcNow;

            Assert.IsTrue(tokenExpiresIn.TotalHours > 9);
            Assert.IsTrue(tokenExpiresIn.TotalHours <= 10);
        }

        [TestMethod()]
        [ExpectedException(typeof(WebException))]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsTestReuseInvalidToken()
        {
            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            MediaServicesCredentials credentials = context1.Credentials;
            credentials.AccessToken = "Invalid";

            try
            {
                var context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext(credentials);
            }
            catch (WebException x)
            {
                var code = ((HttpWebResponse)x.Response).StatusCode;
                Assert.AreEqual(HttpStatusCode.Unauthorized, code);
                throw;
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(WebException))]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsTestReuseInvalidTokenBytes()
        {
            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            MediaServicesCredentials credentials = context1.Credentials;

            string testAcsResponse = "{\"token_type\":\"http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0\",\"access_token\":\"InvalidToken\",\"expires_in\":\"36000\",\"scope\":\"urn:Nimbus\"}";
            byte[] acsResponse = new System.Text.UTF8Encoding().GetBytes(testAcsResponse);
            credentials.SetAcsToken(acsResponse);

            try
            {
                var context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext(credentials);
            }
            catch (WebException x)
            {
                var code = ((HttpWebResponse)x.Response).StatusCode;
                Assert.AreEqual(HttpStatusCode.Unauthorized, code);
                throw;
            }
        }
    }
}
