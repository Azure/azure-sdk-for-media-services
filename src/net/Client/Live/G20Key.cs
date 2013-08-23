// Copyright 2012 Microsoft Corporation
// 
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

using System;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Information about G20 authentication key.
    /// </summary>
    public class G20Key
    {
        /// <summary>
        /// Expiration of the key.
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Key identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Key base 64 representation.
        /// </summary>
        public string Base64Key { get; set; }
    }
}
