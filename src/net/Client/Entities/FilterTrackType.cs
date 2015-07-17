//-----------------------------------------------------------------------
// <copyright file="FilterTrackType.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Type of Track
    /// </summary>
    public enum FilterTrackType
    {
        /// <summary>
        /// Audio
        /// </summary>
        Audio = 0,

        /// <summary>
        /// Video
        /// </summary>
        Video = 1,

        /// <summary>
        /// Text
        /// </summary>
        Text = 2,
    }
}
