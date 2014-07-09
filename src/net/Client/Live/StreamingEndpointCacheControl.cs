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
    /// Describes streaming endpoint cache control.
    /// </summary>
    public class StreamingEndpointCacheControl
    {
        /// <summary>
        /// Gets or sets maximum age of the cache.
        /// </summary>
        public TimeSpan? MaxAge { get; set; }
    }

    /// <summary>
    /// Describes streaming endpoint cache control object in the REST.
    /// </summary>
    internal class StreamingEndpointServiceCacheControl
    {
        /// <summary>
        /// Gets or sets maximum age of the cache.
        /// </summary>
        public long? MaxAge { get; set; }

        /// <summary>
        /// Creates an instance of StreamingEndpointServiceCacheControl class.
        /// </summary>
        public StreamingEndpointServiceCacheControl() {}

        /// <summary>
        /// Creates an instance of StreamingEndpointServiceCacheControl class from an instance of StreamingEndpointCacheControl.
        /// </summary>
        /// <param name="cacheControl">StreamingEndpointCacheControl to copy into newly created instance.</param>
        public StreamingEndpointServiceCacheControl(StreamingEndpointCacheControl cacheControl)
        {
            if (cacheControl == null) return;

            if (cacheControl.MaxAge.HasValue)
            {
                MaxAge = (long) cacheControl.MaxAge.Value.TotalSeconds;
            }
        }

        /// <summary>
        /// Casts StreamingEndpointServiceCacheControl to StreamingEndpointCacheControl.
        /// </summary>
        /// <param name="cacheControl">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator StreamingEndpointCacheControl(StreamingEndpointServiceCacheControl cacheControl)
        {
            if (cacheControl == null)
            {
                return null;
            }

            var result = new StreamingEndpointCacheControl();

            if (cacheControl.MaxAge.HasValue)
            {
                result.MaxAge = TimeSpan.FromSeconds(cacheControl.MaxAge.Value);
            }

            return result;
        }
    }
}
