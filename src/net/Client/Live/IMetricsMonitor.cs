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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Interface for channel or origin metrics monitor
    /// </summary>
    public interface IMetricsMonitor<T> : IStreamingMonitor
    {
        /// <summary>
        /// EventHandler for the all channel metrics received
        /// </summary>
        event EventHandler<MetricsEventArgs<T>> MetricsReceived;

        /// <summary>
        /// Subscribe an event handler to the monitor for a specific channel or origin 
        /// </summary>
        /// <param name="id">Channel or Origin ID</param>
        /// <param name="metricsReceived">Metric received event handler</param>
        void Subscribe(string id, EventHandler<MetricsEventArgs<T>> metricsReceived);

        /// <summary>
        /// Unsubscribe an event handler to the monitor for a specific channel or origin
        /// </summary>
        /// <param name="id">Channel or Origin ID</param>
        /// <param name="metricsReceived">Metric received event handler</param>
        void Unsubscribe(string id, EventHandler<MetricsEventArgs<T>> metricsReceived);
    }
}
