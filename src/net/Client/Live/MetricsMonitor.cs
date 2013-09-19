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
using System.Globalization;
using System.Timers;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// MetricMonitor base class for both origin and live
    /// </summary>
    public abstract class MetricsMonitor
    {
        private Timer _timer;
        private TimeSpan _timerFrequency;

        /// <summary>
        /// Set the metric retrieval frequency
        /// </summary>
        public void SetFrequency(TimeSpan frequency)
        {
            if (frequency < TimeSpan.FromSeconds(30))
            {
                throw new ArgumentOutOfRangeException(
                    "frequency",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        StringTable.MetricMonitoringFrequencyOutOfRange,
                        30));
            }

            _timerFrequency = frequency;
        }

        /// <summary>
        /// Start to monitor metrics and trigger events
        /// </summary>
        public void Start()
        {
            if (_timer != null)
            {
                throw new InvalidOperationException(StringTable.MetricMonitoringAlreadyStartedError);
            }

            var defaultFrequency = TimeSpan.FromSeconds(30);

            if (_timerFrequency < defaultFrequency)
            {
                _timerFrequency = defaultFrequency;
            }

            _timer = new Timer {Interval = _timerFrequency.TotalMilliseconds};
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false;
            _timer.Start();
        }

        /// <summary>
        /// Start to monitor metrics with specified frequency
        /// </summary>
        /// <param name="frequency">moniotring frequency</param>
        public void Start(TimeSpan frequency)
        {
            SetFrequency(frequency);

            Start();
        }

        /// <summary>
        /// Stop to monitor metrics and release the timer
        /// </summary>
        public void Stop()
        {
            var timer = _timer;
            _timer = null;

            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        /// <summary>
        /// Get the list of Metrics and
        /// </summary>
        /// <returns></returns>
        protected abstract void GetMetrics();

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                GetMetrics();
            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException) throw;
            }
            finally
            {
                _timer.Interval = _timerFrequency.TotalMilliseconds;
                _timer.Start();
            }
        }
    }
}
