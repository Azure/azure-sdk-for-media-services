//-----------------------------------------------------------------------
// <copyright file="RSATokenVerificationKey.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1")]
    public class RSATokenVerificationKey:AsymmetricTokenVerificationKey
    {

        private JObject _key;
        private static object _lock = new object();

        public void InitFromRSAParameters(RSAParameters parameters)
        {
            _key =
                new JObject(
                    new JProperty("e", Convert.ToBase64String(parameters.Exponent)),
                    new JProperty("n", Convert.ToBase64String(parameters.Modulus)));
            lock (_lock)
            {
                RawBody = Encoding.UTF8.GetBytes(_key.ToString());
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public override byte[] RawBody
        {
            get
            {
                return base.RawBody;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("RawBody is null");
                }
                lock (_lock)
                {
                    base.RawBody = value;
                }

            }
        }
        public RSAParameters GetRSAParameters()
        {
            lock (_lock)
            {
                if (_key == null && RawBody != null)
                {
                    _key = JObject.Parse(Encoding.UTF8.GetString(RawBody));
                }
                return
                    new RSAParameters()
                    {
                        Exponent = Convert.FromBase64String(_key.Property("e").Value.ToString()),
                        Modulus = Convert.FromBase64String(_key.Property("n").Value.ToString())
                    };
            }


        }
    }
}