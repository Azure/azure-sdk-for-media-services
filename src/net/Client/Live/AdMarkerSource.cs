// Copyright 2014 Microsoft Corporation
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


namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// An enum for the options allowed for an ad marker source.
    /// </summary>
    public enum AdMarkerSource
    {
        /// <summary>
        /// Programatically insers the ads in the live stream.
        /// </summary>
        Api = 0,

        /// <summary>
        /// Use the SCTE-35 signalling to insert the ads. Works for RTPMPEG2-TS channels only.
        /// </summary>
        Scte35 = 1
    }
}
