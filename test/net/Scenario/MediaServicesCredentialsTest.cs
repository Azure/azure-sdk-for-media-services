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
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

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
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void MediaServicesCredentialsTestReuseToken()
        {
            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            Assert.IsNull(context1.Credentials.AccessToken);
            //In order to obtain token REST call need to be issued
            MakeRestCallAndVerifyToken(context1);
            MediaServicesCredentials credentials = new MediaServicesCredentials("whatever", "whatever")
            {
                AccessToken = context1.Credentials.AccessToken,
                TokenExpiration = context1.Credentials.TokenExpiration
            };

            var context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext(credentials);
            context2.Assets.FirstOrDefault();
        }

        [TestMethod()]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void MediaServicesThrowsArgumentExceptionOnNull()
        {
            string nonNull = "nonNull";

            string[] argumentNames = {"clientId", "clientSecret", "scope", "acsBaseAddress" };
            string[,] arguments = {{null, nonNull, nonNull, nonNull},
                                   {nonNull, null, nonNull, nonNull},
                                   {nonNull, nonNull, null, nonNull},
                                   {nonNull, nonNull, nonNull, null}};

            for (int i = 0; i < 4; i++)
            {
                try
                {
                    MediaServicesCredentials creds = new MediaServicesCredentials(arguments[i, 0], arguments[i, 1], arguments[i, 2], arguments[i, 3]);
                }
                catch (ArgumentException ae)
                {
                    string expectedMessage = string.Format(StringTable.ErrorArgCannotBeNullOrEmpty, argumentNames[i]);
                    Assert.AreEqual(expectedMessage, ae.Message);
                }
            }
        }

        private static void MakeRestCallAndVerifyToken(CloudMediaContext context)
        {
            context.Assets.FirstOrDefault();
            Assert.IsNotNull(context.Credentials.AccessToken);
        }

        [TestMethod()]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void MediaServicesCredentialsTestTokenReaquire()
        {
            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            string account = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountName;
            string key = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountKey;
            MediaServicesCredentials credentials = new MediaServicesCredentials(account, key)
            {
                AccessToken = context1.Credentials.AccessToken,
                TokenExpiration = DateTime.UtcNow.AddYears(-1),
                Scope = context1.Credentials.Scope,
                AcsBaseAddress = context1.Credentials.AcsBaseAddress
            };

            var context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext(credentials);
            MakeRestCallAndVerifyToken(context2);

            Assert.IsTrue(context2.Credentials.TokenExpiration > DateTime.UtcNow.AddMinutes(-10));

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

            var token = target.GetAccessToken();
            Assert.AreEqual( target.AccessToken, token.Item1);
            Assert.AreEqual(target.TokenExpiration, token.Item2);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void MediaServicesCredentialsTestAuthorizationHeader()
        {
            var credentials = WindowsAzureMediaServicesTestConfiguration.CreateMediaServicesCredentials();
            var header = credentials.GetAuthorizationHeader();
            Assert.IsNotNull(header);
            Assert.AreEqual("Bearer " + credentials.AccessToken, header);
        }

        [TestMethod()]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsPassingAcsEndPoint()
        {
            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            string account = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountName;
            string key = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountKey;
            MediaServicesCredentials credentials = new MediaServicesCredentials(account, key,WindowsAzureMediaServicesTestConfiguration.MediaServicesAccessScope,WindowsAzureMediaServicesTestConfiguration.MediaServicesAcsBaseAddress);
            Assert.IsNull(credentials.AccessToken);
            Assert.IsTrue(credentials.TokenExpiration < DateTime.UtcNow);

            credentials.RefreshToken();

            Assert.IsNotNull(credentials.AccessToken);
            Assert.IsTrue(credentials.AccessToken.Length > 0);
            Assert.IsTrue(credentials.TokenExpiration > DateTime.UtcNow);
        }

        [TestMethod()]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsAcsEndPointListWithWrongOneFirst()
        {
            string account = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountName;
            string key = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountKey;
            MediaServicesCredentials credentials = new MediaServicesCredentials(account, key, WindowsAzureMediaServicesTestConfiguration.MediaServicesAccessScope, new List<string> { "http://dummyacsendpoint", WindowsAzureMediaServicesTestConfiguration.MediaServicesAcsBaseAddress});
            Assert.IsNull(credentials.AccessToken);
            Assert.IsTrue(credentials.TokenExpiration < DateTime.UtcNow);

            credentials.RefreshToken();

            Assert.IsNotNull(credentials.AccessToken);
            Assert.IsTrue(credentials.AccessToken.Length > 0);
            Assert.IsTrue(credentials.TokenExpiration > DateTime.UtcNow);
            
        }


        [TestMethod()]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsAcsEndPointListWithWrongOneLast()
        {
            string account = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountName;
            string key = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountKey;
            MediaServicesCredentials credentials = new MediaServicesCredentials(account, key, WindowsAzureMediaServicesTestConfiguration.MediaServicesAccessScope, new List<string> { WindowsAzureMediaServicesTestConfiguration.MediaServicesAcsBaseAddress, "http://dummyacsendpoint"});
            Assert.IsNull(credentials.AccessToken);
            Assert.IsTrue(credentials.TokenExpiration < DateTime.UtcNow);

            credentials.RefreshToken();

            Assert.IsNotNull(credentials.AccessToken);
            Assert.IsTrue(credentials.AccessToken.Length > 0);
            Assert.IsTrue(credentials.TokenExpiration > DateTime.UtcNow);
        }

        [TestMethod()]
        [ExpectedException(typeof(WebException))]
        [TestCategory("DailyBvtRun")]
        public void MediaServicesCredentialsAcsEndPointCustomRetryPolicy()
        {
            string account = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountName;
            string key = WindowsAzureMediaServicesTestConfiguration.MediaServiceAccountKey;
            var retryStrategy = new Incremental(1, TimeSpan.FromSeconds(1),TimeSpan.FromSeconds(2));
            var errorDetectionStrategy = new ErrorDetectionStrategyForRefreshToken();
            MediaServicesCredentials credentials = new MediaServicesCredentials(account, key,
                WindowsAzureMediaServicesTestConfiguration.MediaServicesAccessScope,
                new List<string>
                {
                    "http://dummyacsendpoint"
                })
            {

                RefreshTokenRetryPolicy = new RetryPolicy(errorDetectionStrategy, retryStrategy)
            };

            Assert.IsNull(credentials.AccessToken);
            Assert.IsTrue(credentials.TokenExpiration < DateTime.UtcNow);
            try
            {
                credentials.RefreshToken();
            }
            catch (WebException)
            {
                Assert.IsTrue(errorDetectionStrategy.Invoked);
                throw;
            }
        }

        [TestMethod()]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void MediaServicesCredentialsTestSetAcsToken()
        {
            MediaServicesCredentials target = new MediaServicesCredentials("dummyClientId", "dummyClientSecret", "dummyScope", "dummyAcsBaseAddress");

            string testAcsResponse = "{\"token_type\":\"http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0\",\"access_token\":\"http%3a%2f%2fschemas.xmlsoap.org%2fws%2f2005%2f05%2fidentity%2fclaims%2fnameidentifier=mediacreator&urn%3aSubscriptionId=3c5e503f-adcb-4aa5-a549-f34931566d6c&http%3a%2f%2fschemas.microsoft.com%2faccesscontrolservice%2f2010%2f07%2fclaims%2fidentityprovider=https%3a%2f%2fwamsprodglobal001acs.accesscontrol.windows.net%2f&Audience=urn%3aWindowsAzureMediaServices&ExpiresOn=1386947515&Issuer=https%3a%2f%2fwamsprodglobal001acs.accesscontrol.windows.net%2f&HMACSHA256=8RMeaHPfHHWAqlDSAvg0YDOpYhzjBGAsKZMMNeAwLsE%3d\",\"expires_in\":\"5999\",\"scope\":\"urn:WindowsAzureMediaServices\"}";
            string expectedToken = "http%3a%2f%2fschemas.xmlsoap.org%2fws%2f2005%2f05%2fidentity%2fclaims%2fnameidentifier=mediacreator&urn%3aSubscriptionId=3c5e503f-adcb-4aa5-a549-f34931566d6c&http%3a%2f%2fschemas.microsoft.com%2faccesscontrolservice%2f2010%2f07%2fclaims%2fidentityprovider=https%3a%2f%2fwamsprodglobal001acs.accesscontrol.windows.net%2f&Audience=urn%3aWindowsAzureMediaServices&ExpiresOn=1386947515&Issuer=https%3a%2f%2fwamsprodglobal001acs.accesscontrol.windows.net%2f&HMACSHA256=8RMeaHPfHHWAqlDSAvg0YDOpYhzjBGAsKZMMNeAwLsE%3d";
            long expectedTicks = 635225443150000000;
            byte[] acsResponse = new UTF8Encoding().GetBytes(testAcsResponse);
            target.SetAcsToken(acsResponse);

            Assert.AreEqual(expectedToken, target.AccessToken);
            Assert.AreEqual(expectedTicks, target.TokenExpiration.Ticks);
        }

        [TestMethod()]
        [ExpectedException(typeof(DataServiceQueryException))]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void MediaServicesCredentialsTestReuseInvalidToken()
        {
            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            //Make call to get endpoint and valid token
            MakeRestCallAndVerifyToken(context1);
            MediaServicesCredentials credentials = context1.Credentials;
            credentials.AccessToken = "Invalid";

            try
            {
                var context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext(credentials);
                MakeRestCallAndVerifyToken(context2);
            }
            catch (DataServiceQueryException x)
            {
                var code = x.Response.StatusCode;
                Assert.AreEqual((int)HttpStatusCode.Unauthorized, code);
                throw;
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(DataServiceQueryException))]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void MediaServicesCredentialsTestReuseInvalidTokenBytes()
        {
            // Get the current time plus two hours and then remove the milliseconds component
            // since the encoded expiry is represented in seconds
            DateTime timeToEncode = DateTime.UtcNow.AddSeconds(5999);
            timeToEncode = timeToEncode.Subtract(TimeSpan.FromMilliseconds(timeToEncode.Millisecond));

            var context1 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            MakeRestCallAndVerifyToken(context1);
            MediaServicesCredentials credentials = context1.Credentials;

            StringBuilder builder = new StringBuilder();
            builder.Append("{\"token_type\":\"http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0\",\"access_token\":\"http%3a%2f%2fschemas.xmlsoap.org%2fws%2f2005%2f05%2fidentity%2fclaims%2fnameidentifier=mediacreator&urn%3aSubscriptionId=3c5e503f-adcb-4aa5-a549-f34931566d6c&http%3a%2f%2fschemas.microsoft.com%2faccesscontrolservice%2f2010%2f07%2fclaims%2fidentityprovider=https%3a%2f%2fwamsprodglobal001acs.accesscontrol.windows.net%2f&Audience=urn%3aWindowsAzureMediaServices&ExpiresOn=");
            builder.Append(EncodeExpiry(timeToEncode));
            builder.Append("&Issuer=https%3a%2f%2fwamsprodglobal001acs.accesscontrol.windows.net%2f&HMACSHA256=8RMeaHPfHHWAqlDSAvg0YDOpYhzjBGAsKZMMNeAwLsE%3d\",\"expires_in\":\"5999\",\"scope\":\"urn:WindowsAzureMediaServices\"}");
            string badAcsResponse = builder.ToString();
            byte[] acsResponse = new UTF8Encoding().GetBytes(badAcsResponse);
            credentials.SetAcsToken(acsResponse);

            Assert.AreEqual(timeToEncode.ToString(), credentials.TokenExpiration.ToString());

            try
            {
                var context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext(credentials);
                MakeRestCallAndVerifyToken(context2);
            }
            catch (DataServiceQueryException x)
            {
                var code = x.Response.StatusCode;
                Assert.AreEqual((int)HttpStatusCode.Unauthorized, code);
                throw;
            }
        }

        private static readonly DateTime TokenBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        // Note that this function can only encode the time to the granularity of a second.
        // Thus you should remove or ignore the millisecond component of the DateTime if you
        // plan to compare the DateTime values from encoding and decoding.
        private static string EncodeExpiry(DateTime timeToEncode)
        {
            TimeSpan timeRelativeToBase = timeToEncode.Subtract(TokenBaseTime);

            return Convert.ToInt64(timeRelativeToBase.TotalSeconds).ToString();
        }
    }
}
