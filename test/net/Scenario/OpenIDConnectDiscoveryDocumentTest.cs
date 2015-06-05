//-----------------------------------------------------------------------
// <copyright file="OpenIDConnectDiscoveryDocumentTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.IdentityModel.Protocols;
using System.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class OpenIDConnectDiscoveryDocumentTest
    {
        private const string googleOpenConectDiscoveryUri = "https://accounts.google.com/.well-known/openid-configuration";
        private const string adOpenConectDiscoveryUri = "https://login.windows.net/common/.well-known/openid-configuration";
        
         [TestMethod]
        public void FetchGooleJWKKeysAndUseIdentityExtensions()
        {
            
            GetAndVerifyJsonWebKeys(googleOpenConectDiscoveryUri);

            
        }
        [TestMethod]
         public void FetchMicrosoftJWKKeysAndUseIdentityExtensions()
         {
             GetAndVerifyJsonWebKeys(adOpenConectDiscoveryUri);

         }

         private static IList<SecurityToken> GetAndVerifyJsonWebKeys(string uri)
         {
             Microsoft.IdentityModel.Protocols.JsonWebKey key = new IdentityModel.Protocols.JsonWebKey();
             Microsoft.IdentityModel.Protocols.OpenIdConnectConfiguration config;
             System.Threading.CancellationTokenSource src = new System.Threading.CancellationTokenSource();
             config = Microsoft.IdentityModel.Protocols.OpenIdConnectConfigurationRetriever.GetAsync(uri, src.Token).Result;
             JsonWebKeySet keyset = config.JsonWebKeySet;
             Assert.IsNotNull(keyset);
             Assert.IsNotNull(keyset.GetSigningTokens());
             return keyset.GetSigningTokens();
         }

         

       

        
    }
}