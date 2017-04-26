//-----------------------------------------------------------------------
// <copyright file="FairPlayConfiguration.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MediaServices.Client.FairPlay
{
    /// <summary>
    /// Configuration for FairPlay Authorization Policy Option.
    /// Key Delivery will use these values for generating FairPlay CKC.
    /// </summary>
    public class FairPlayConfiguration
    {
        /// <summary>
        /// Id of the Key that must be used as FairPlay ASk.
        /// </summary>
        public Guid ASkId { get; set; }

        /// <summary>
        /// Id of the Password Key encrypting FairPlay certificate in PKCS 12 (pfx) format.
        /// </summary>
        public Guid FairPlayPfxPasswordId { get; set; }

        /// <summary>
        /// Base64 representation of FairPlay certificate in PKCS 12 (pfx) format (including private key).
        /// </summary>
        public string FairPlayPfx { get; set; }

        /// <summary>
        /// Initialization Vector in hex used for encrypting protected content.
        /// </summary>
        public string ContentEncryptionIV { get; set; }

        public RentalAndLeaseKeyType RentalAndLeaseKeyType { get; set; }

        public uint RentalDuration { get; set; }

        /// <summary>
        /// Creates a string that can be used as FairPlay Policy Option Configuration.
        /// </summary>
        /// <param name="appCertificate">FairPlay application certificate.</param>
        /// <param name="pfxPassword">Password protecting FairPlay application certificate.</param>
        /// <param name="pfxPasswordKeyId">Id of the key storing the password protecting 
        /// FairPlay application certificate.</param>
        /// <param name="askId">Id of the FairPlay Aplication Secret key.</param>
        /// <param name="contentIv">Initialization Vector used for encrypting the content.</param>
        /// <returns>String that can be used as FairPlay Policy Option Configuration.</returns>
        public static string CreateSerializedFairPlayOptionConfiguration(
            X509Certificate2 appCertificate,
            string pfxPassword,
            Guid pfxPasswordKeyId,
            Guid askId,
            byte[] contentIv) 
        {
            return CreateSerializedFairPlayOptionConfiguration(
                appCertificate,
                pfxPassword,
                pfxPasswordKeyId,
                askId,
                contentIv, 
                RentalAndLeaseKeyType.Undefined, 
                0);
        }

        /// <summary>
        /// Creates a string that can be used as FairPlay Policy Option Configuration.
        /// </summary>
        /// <param name="appCertificate">FairPlay application certificate.</param>
        /// <param name="pfxPassword">Password protecting FairPlay application certificate.</param>
        /// <param name="pfxPasswordKeyId">Id of the key storing the password protecting 
        /// FairPlay application certificate.</param>
        /// <param name="askId">Id of the FairPlay Aplication Secret key.</param>
        /// <param name="contentIv">Initialization Vector used for encrypting the content.</param>
        /// <param name="rentalAndLeaseKeyType">Rental and lease KeyType.</param>
        /// <param name="rentalDuration">Rental duration in seconds.</param>
        /// <returns>String that can be used as FairPlay Policy Option Configuration.</returns>
        public static string CreateSerializedFairPlayOptionConfiguration(
            X509Certificate2 appCertificate,
            string pfxPassword,
            Guid pfxPasswordKeyId,
            Guid askId,
            byte[] contentIv,
            RentalAndLeaseKeyType rentalAndLeaseKeyType,
            uint rentalDuration)
        {
            byte[] certificateBytes = appCertificate.Export(X509ContentType.Pfx, pfxPassword);
            string certString = Convert.ToBase64String(certificateBytes);

            string ivString = BitConverter.ToString(contentIv).Replace("-", string.Empty);

            var config = new FairPlayConfiguration
            {
                ASkId = askId,
                ContentEncryptionIV = ivString,
                FairPlayPfx = certString,
                FairPlayPfxPasswordId = pfxPasswordKeyId
            };

            if (rentalAndLeaseKeyType != RentalAndLeaseKeyType.Undefined)
            {
                config.RentalAndLeaseKeyType = rentalAndLeaseKeyType;
                config.RentalDuration = rentalDuration;
            }

            string configuration = JsonConvert.SerializeObject(config);

            return configuration;
        }
    }
}
