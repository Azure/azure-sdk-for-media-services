//-----------------------------------------------------------------------
// <copyright file="ErrorDetail.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Channel Metrics EventArgs returned to listeners.
    /// </summary>
    public class MetricsEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The list of metrics data for Channel(s) or Origins(s)
        /// There is only one element if monitoring a single channel or origin
        /// </summary>
        public ReadOnlyCollection<T> Metrics { get; internal set; }
    }
}
