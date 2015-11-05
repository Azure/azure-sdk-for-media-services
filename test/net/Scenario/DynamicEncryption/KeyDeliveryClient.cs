//-----------------------------------------------------------------------
// <copyright file="KeyDeliveryServiceClient.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Practices.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.DynamicEncryption
{
    public class KeyDeliveryServiceClient
    {
        private readonly RetryPolicy _retryPolicy;
        private const string AuthenticationSchema = "Bearer";

        public KeyDeliveryServiceClient(RetryPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy;
        }

        public byte[] AcquireHlsKeyWithBearerHeader(Uri keydeliveryUri, string authToken)
        {
            var kdClient = new HttpClient();
            kdClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchema, authToken);
            return GetKey(keydeliveryUri, kdClient);
        }

        public byte[] AcquireWidevineLicenseWithBearerHeader(Uri keydeliveryUri, string authToken, byte[] licenseChallenge)
        {
            var kdClient = new HttpClient();
            kdClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationSchema, authToken);

            return _retryPolicy.ExecuteAction(() =>
            {
                HttpResponseMessage response = kdClient.PostAsync(keydeliveryUri, new ByteArrayContent(licenseChallenge)).Result;
                response.EnsureSuccessStatusCode();
                byte[] hlsContentKey = response.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    Stream stream = t.Result;
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    return bytes;
                }).Result;

                return hlsContentKey;
            });
        }

        private byte[] GetKey(Uri keydeliveryUri, HttpClient kdClient)
        {
            return _retryPolicy.ExecuteAction(() =>
            {
                HttpResponseMessage response = kdClient.PostAsync(keydeliveryUri, new StringContent(string.Empty)).Result;
                response.EnsureSuccessStatusCode();
                byte[] hlsContentKey = response.Content.ReadAsStreamAsync().ContinueWith(t =>
                {
                    Stream stream = t.Result;
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int) stream.Length);
                    return bytes;
                }).Result;

                return hlsContentKey;
            });
        }
    }
}