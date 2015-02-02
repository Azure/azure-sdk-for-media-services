// Copyright 2015 Microsoft Corporation
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
    /// Specifies a video stream.
    /// </summary>
    public class VideoStream
    {
        /// <summary>
        /// Gets or sets the stream index when source from MPEG2-TS.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of video stream.
        /// </summary>
        public string Name { get; set; }
    }
}
