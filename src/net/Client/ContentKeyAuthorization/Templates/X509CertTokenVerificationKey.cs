//-----------------------------------------------------------------------
// <copyright file="X509CertTokenVerificationKey.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1")]
    public class X509CertTokenVerificationKey : AsymmetricTokenVerificationKey,IDisposable
    {
        private static object _lock = new object();
        private X509Certificate2 _x509Certificate;
        private X509SecurityToken _securityToken;
        private bool _disposed = false;

        public X509CertTokenVerificationKey()
        {

        }

        public X509CertTokenVerificationKey(X509Certificate2 cert)
        {
           
                lock (_lock)
                {
                    if (_x509Certificate == null)
                    {
                        _x509Certificate = cert;
                        _securityToken = new X509SecurityToken(_x509Certificate);
                        base.RawBody = _x509Certificate.RawData;
                    }
                }
            
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
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

        public X509Certificate2 X509Certificate2
        {
            get
            {
                if (this._x509Certificate != null)
                {
                    return _x509Certificate;
                }

                if (this.RawBody == null) return null;
                InitCertAndToken();
                return _x509Certificate;
                
            }
        }

        public X509SecurityToken X509SecurityToken
        {
            get
            {
                if (this._securityToken != null)
                {
                    return _securityToken;
                }

                if (this.RawBody == null) return null;
                InitCertAndToken();
                return _securityToken;
            }
        }

        private void InitCertAndToken()
        {
            if (_x509Certificate == null)
            {
                lock (_lock)
                {
                    if (_x509Certificate == null)
                    {
                        _x509Certificate = new X509Certificate2(this.RawBody);
                        _securityToken = new X509SecurityToken(_x509Certificate);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
                
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                lock (_lock)
                {
                    if (_securityToken != null)
                    {
                        _securityToken.Dispose();
                    }
                    _x509Certificate = null;
                }
                _disposed = true;
            }
        }
    }
}