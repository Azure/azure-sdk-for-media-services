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
    /// Describes state of a program.
    /// </summary>
    public enum ProgramState
    {
        /// <summary>
        /// Program is stopped
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// Program is starting
        /// </summary>
        Starting = 1,

        /// <summary>
        /// Program is running
        /// </summary>
        Running = 2,

        /// <summary>
        /// Program is stopping
        /// </summary>
        Stopping = 3
    }
}
