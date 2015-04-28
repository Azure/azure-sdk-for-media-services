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
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Moq;
using Microsoft.IdentityModel.Protocols;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class OpenIDConnectDiscoveryDocumentTest
    {       
    
        [TestMethod]
        public void FetchJWKKeyAsRSATokenVerificationKey()
        {
            string keysResponse = @"{
             ""keys"": [
              {
               ""kty"": ""RSA"",
               ""alg"": ""RS256"",
               ""use"": ""sig"",
               ""kid"": ""d83ad1bbaac388a4dcb4957e53282a0e7d0bf98a"",
               ""n"": ""temCVy5oxWCEUwHUqGXxvHnySlaZtT_JmHm0zICxsanboYss-b3nOqIXN45L5TyTiNbOBgE6vge2TfIjo_NqXBiKjRNl_g7F4iwl07p2abn3KQ6mgYDlFMhJJOXG4-0dMORBgi3hQi8VajLHJ04FoorZsf__FDb1gvvnPObUQwM="",
               ""e"": ""AQAB""
              }
             ]
            }";

            JsonWebKey jsonWebKey = FetchAndValidateJsonWebKeyWithCommonProperties(keysResponse);
            Assert.AreEqual(0,jsonWebKey.X5c.Count, "x5c should not contain elements ");
            Assert.IsNull(jsonWebKey.X5t, "x5t field should be null");

            RSATokenVerificationKey tokenVerificationKey = jsonWebKey.AsTokenVerificationKey() as RSATokenVerificationKey;
            Assert.IsNotNull(tokenVerificationKey);
            RSAParameters parameters = tokenVerificationKey.GetRSAParameters();
            Assert.IsNotNull(parameters);
            Assert.IsNotNull(parameters.Modulus);
            Assert.IsNotNull(parameters.Exponent);
        }

        [TestMethod]
        public void FetchAsTokenVerificationKey()
        {
            string keysResponse = @"{
             ""keys"": [
              {
               ""kty"": ""RSA"",
               ""alg"": ""RS256"",
               ""use"": ""sig"",
               ""kid"": ""d83ad1bbaac388a4dcb4957e53282a0e7d0bf98a"",
               ""n"": ""temCVy5oxWCEUwHUqGXxvHnySlaZtT_JmHm0zICxsanboYss-b3nOqIXN45L5TyTiNbOBgE6vge2TfIjo_NqXBiKjRNl_g7F4iwl07p2abn3KQ6mgYDlFMhJJOXG4-0dMORBgi3hQi8VajLHJ04FoorZsf__FDb1gvvnPObUQwM="",
               ""e"": ""AQAB""
              }
             ]
            }";

            Assert.IsNotNull(FetchAndValidateJsonWebKeyWithCommonProperties(keysResponse).AsTokenVerificationKey());

           
        }

        private static JsonWebKey FetchAndValidateJsonWebKeyWithCommonProperties(string keysResponse)
        {
                    
           
            Microsoft.IdentityModel.Protocols.OpenIdConnectConfiguration config;
            System.Threading.CancellationTokenSource src = new System.Threading.CancellationTokenSource();
            TestDocumentRetriever retriver = new TestDocumentRetriever("{\"jwks_uri\": \"secondary\"}", keysResponse);            
            
            config = OpenIdConnectConfigurationRetriever.GetAsync("primary",retriver as IDocumentRetriever, src.Token).Result;
                      
            Assert.IsNotNull(config.JsonWebKeySet);
            Assert.IsNotNull(config.JsonWebKeySet.Keys);
            Assert.AreEqual(1, config.JsonWebKeySet.Keys.Count);
            JsonWebKey jsonWebKey = config.JsonWebKeySet.Keys.ToList()[0];
            ValidateCommonJWKProperties(jsonWebKey);
            return jsonWebKey;
        }

        private static void ValidateCommonJWKProperties(JsonWebKey jsonWebKey)
        {
            Assert.IsNotNull(jsonWebKey.Kid);
            Assert.IsNotNull(jsonWebKey.Kty);
            Assert.IsNotNull(jsonWebKey.E);
            Assert.IsNotNull(jsonWebKey.N);
            Assert.AreEqual("sig", jsonWebKey.Use);
        }      

       

        [TestMethod]
        public void FetchJWKKeyAsX509TokenVerificationKey()
        {
            string keysResponse = @"
             {
                ""keys"":
                [
                    {
                    ""kty"":""RSA"",
                    ""use"":""sig"",
                    ""kid"":""kriMPdmBvx68skT8-mPAB3BseeA"",
                    ""x5t"":""kriMPdmBvx68skT8-mPAB3BseeA"",
                    ""n"":""kSCWg6q9iYxvJE2NIhSyOiKvqoWCO2GFipgH0sTSAs5FalHQosk9ZNTztX0ywS/AHsBeQPqYygfYVJL6/EgzVuwRk5txr9e3n1uml94fLyq/AXbwo9yAduf4dCHTP8CWR1dnDR+Qnz/4PYlWVEuuHHONOw/blbfdMjhY+C/BYM2E3pRxbohBb3x//CfueV7ddz2LYiH3wjz0QS/7kjPiNCsXcNyKQEOTkbHFi3mu0u13SQwNddhcynd/GTgWN8A+6SN1r4hzpjFKFLbZnBt77ACSiYx+IHK4Mp+NaVEi5wQtSsjQtI++XsokxRDqYLwus1I1SihgbV/STTg5enufuw=="",
                    ""e"":""AQAB"",
                    ""x5c"":[""MIIDPjCCAiqgAwIBAgIQsRiM0jheFZhKk49YD0SK1TAJBgUrDgMCHQUAMC0xKzApBgNVBAMTImFjY291bnRzLmFjY2Vzc2NvbnRyb2wud2luZG93cy5uZXQwHhcNMTQwMTAxMDcwMDAwWhcNMTYwMTAxMDcwMDAwWjAtMSswKQYDVQQDEyJhY2NvdW50cy5hY2Nlc3Njb250cm9sLndpbmRvd3MubmV0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkSCWg6q9iYxvJE2NIhSyOiKvqoWCO2GFipgH0sTSAs5FalHQosk9ZNTztX0ywS/AHsBeQPqYygfYVJL6/EgzVuwRk5txr9e3n1uml94fLyq/AXbwo9yAduf4dCHTP8CWR1dnDR+Qnz/4PYlWVEuuHHONOw/blbfdMjhY+C/BYM2E3pRxbohBb3x//CfueV7ddz2LYiH3wjz0QS/7kjPiNCsXcNyKQEOTkbHFi3mu0u13SQwNddhcynd/GTgWN8A+6SN1r4hzpjFKFLbZnBt77ACSiYx+IHK4Mp+NaVEi5wQtSsjQtI++XsokxRDqYLwus1I1SihgbV/STTg5enufuwIDAQABo2IwYDBeBgNVHQEEVzBVgBDLebM6bK3BjWGqIBrBNFeNoS8wLTErMCkGA1UEAxMiYWNjb3VudHMuYWNjZXNzY29udHJvbC53aW5kb3dzLm5ldIIQsRiM0jheFZhKk49YD0SK1TAJBgUrDgMCHQUAA4IBAQCJ4JApryF77EKC4zF5bUaBLQHQ1PNtA1uMDbdNVGKCmSf8M65b8h0NwlIjGGGy/unK8P6jWFdm5IlZ0YPTOgzcRZguXDPj7ajyvlVEQ2K2ICvTYiRQqrOhEhZMSSZsTKXFVwNfW6ADDkN3bvVOVbtpty+nBY5UqnI7xbcoHLZ4wYD251uj5+lo13YLnsVrmQ16NCBYq2nQFNPuNJw6t3XUbwBHXpF46aLT1/eGf/7Xx6iy8yPJX4DyrpFTutDz882RWofGEO5t4Cw+zZg70dJ/hH/ODYRMorfXEW+8uKmXMKmX2wyxMKvfiPbTy5LmAU8Jvjs2tLg4rOBcXWLAIarZ""]
                    }
                ]
            }";

            JsonWebKey jsonWebKey = FetchAndValidateJsonWebKeyWithCommonProperties(keysResponse);

            X509CertTokenVerificationKey tokenVerificationKey = jsonWebKey.AsTokenVerificationKey() as X509CertTokenVerificationKey;
            Assert.IsNotNull(tokenVerificationKey);
            Assert.IsNotNull(jsonWebKey.X5c);
            Assert.IsNotNull(jsonWebKey.X5t);
            X509Certificate2 cert = tokenVerificationKey.X509Certificate2;
            Assert.IsNotNull(cert);
           
        }

        private class TestDocumentRetriever : IDocumentRetriever
        {
            private string _primaryDocument;
            private string _secondaryDocument;
            private IDocumentRetriever _fallback;

            public TestDocumentRetriever(string primaryDocument, string secondaryDocument)
            {
                _primaryDocument = primaryDocument;
                _secondaryDocument = secondaryDocument;
            }

            public TestDocumentRetriever(string primaryDocument, IDocumentRetriever fallback)
            {
                _primaryDocument = primaryDocument;
                _fallback = fallback;
            }

            public Task<string> GetDocumentAsync(string address, CancellationToken cancel)
            {
                if (string.Equals("primary", address))
                {
                    return Task.FromResult(_primaryDocument);
                }
                if (string.Equals("secondary", address) && !string.IsNullOrWhiteSpace(_secondaryDocument))
                {
                    return Task.FromResult(_secondaryDocument);
                }
                if (_fallback != null)
                {
                    return _fallback.GetDocumentAsync(address, cancel);
                }
                throw new IOException("Document not found: " + address);
            }
        }
    }
    
}