//-----------------------------------------------------------------------
// <copyright file="ITokenProvider.cs" company="Microsoft">Copyright 2016 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A generic interface to a RFC6750 bearer token provider.
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        ///  The media services account for which the token is being provided.
        /// </summary>
        string MediaServicesAccountName { get; }

        /// <summary>
        /// Gets a value for the Authorization header in RFC6750 format
        /// </summary>
        /// <returns></returns>
        string GetAuthorizationHeader();

        /// <summary>
        /// Gets the access token to use.
        /// </summary>
        /// <returns>A tuple containing access token and its expiration time.</returns>
        Tuple<string, DateTimeOffset> GetAccessToken();
    }
}
