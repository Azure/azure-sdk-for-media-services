//-----------------------------------------------------------------------
// <copyright file="AzureADTokenProviderTest.cs" company="Microsoft">Copyright 2016 Microsoft Corporation</copyright>
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
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{

    /// <summary>
    /// An implementation of ITokenProvider that uses Azure AD to obtain Token.
    /// </summary>
    class AzureADTokenProvider : ITokenProvider
    {
        private AuthenticationContext _context;

        public AzureADTokenProvider()
        {
            string authority = string.Format("{0}/{1}", 
                ConfigurationManager.AppSettings["AadAuthority"], ConfigurationManager.AppSettings["AadTenant"]);
            _context = new AuthenticationContext(authority);
        }

        public string MediaServicesAccountName
        {
            get
            {
                return ConfigurationManager.AppSettings["MediaServiceAccountName"];
            }
        }

        private AuthenticationResult GetToken()
        {
            string upn = string.Format("{0}@{1}", ConfigurationManager.AppSettings["MediaServiceAccountName"], ConfigurationManager.AppSettings["AadTenant"]);
            var credentials = new UserPasswordCredential(upn, ConfigurationManager.AppSettings["MediaServiceAccountKey"]);
            //TODO: May be use the graph API to find the SDK App from the tenant by name or by URI.
            var clientId = ConfigurationManager.AppSettings["AadClientId"];
            var result = _context.AcquireTokenAsync(
                ConfigurationManager.AppSettings["MediaServicesUri"],
                clientId,
                credentials).Result;
            return result;
        }

        public string GetAuthorizationHeader()
        {
            return GetToken().CreateAuthorizationHeader();
        }

        public Tuple<string, DateTimeOffset> GetAccessToken()
        {
            var result = GetToken();
            return new Tuple<string, DateTimeOffset>(result.AccessToken, result.ExpiresOn);
        }
    }

    [TestClass]
    [Ignore]  //Ignored for now till the AAD migration is done.
    public class AzureADTokenProviderTest
    {
        [TestMethod]
        public void TestADTokenProvider()
        {
            var provider = new AzureADTokenProvider();
            var uri = new Uri(ConfigurationManager.AppSettings["MediaServicesUri"]);
            var context = new CloudMediaContext(uri, provider);
            var asset = context.Assets.FirstOrDefault();
        }
    }
}
