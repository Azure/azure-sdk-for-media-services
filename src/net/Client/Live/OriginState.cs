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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the state of an Origin.
    /// </summary>
    public enum OriginState
    {
        /// <summary>
        /// Origin is stopped.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// Origin is starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// Origin is running.
        /// </summary>
        Running = 2,

        /// <summary>
        /// Origin is stopping.
        /// </summary>
        Stopping = 3,

        /// <summary>
        /// Origin is scaling.
        /// </summary>
        Scaling = 4,

        /// <summary>
        /// Origin is being deleted
        /// </summary>
        Deleting = 5,
    }
}
