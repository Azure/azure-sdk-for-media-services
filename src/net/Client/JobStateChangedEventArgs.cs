//-----------------------------------------------------------------------
// <copyright file="JobStateChangedEventArgs.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Describes the change of state of an <see cref="IJob"/> that was submitted.
    /// </summary>
    public class JobStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="previousState">The previous state of the job.</param>
        /// <param name="currentState">The current state of the job.</param>
        public JobStateChangedEventArgs(JobState previousState, JobState currentState)
        {
            this.PreviousState = previousState;
            this.CurrentState = currentState;
        }

        /// <summary>
        /// Gets the previous state of the job.
        /// </summary>
        public JobState PreviousState { get; private set; }

        /// <summary>
        /// Gets the current state of the job.
        /// </summary>
        public JobState CurrentState { get; private set; }

    }
}
