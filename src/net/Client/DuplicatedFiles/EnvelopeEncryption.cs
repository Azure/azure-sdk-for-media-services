﻿//-----------------------------------------------------------------------
// <copyright file="EnvelopeEncryption.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Provides envelope encryption.
    /// </summary>
    public static class EnvelopeEncryption
    {
        /// <summary>
        /// The version of the encryption scheme.
        /// </summary>
        public static readonly string SchemeVersion = "1.0";

        /// <summary>
        /// The name of the encryption scheme.
        /// </summary>
        public static readonly string SchemeName = "EnvelopeEncryption";
    }      
}
