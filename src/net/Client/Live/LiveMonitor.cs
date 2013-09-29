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
    /// A generic monitor class for doing something periodically
    /// </summary>
    public abstract class LiveMonitor : ILiveMonitor
    {
        private Timer _timer;
        private TimeSpan _timerInterval;

        protected abstract TimeSpan DefaultTimerInterval { get; }
       
        /// <summary>
        /// Set the metric retrieval timer interval
        /// </summary>
        public void SetInterval(TimeSpan interval)
        {
            if (interval < DefaultTimerInterval)
            {
                throw new ArgumentOutOfRangeException(
                    "interval",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        StringTable.MetricMonitoringIntervalOutOfRange,
                        30));
            }

            _timerInterval = interval;
        }

        /// <summary>
        /// Start to monitor metrics and trigger events
        /// </summary>
        protected void Start()
        {
            if (_timer != null)
            {
                //already started, just return;
                return;
            }

            if (_timerInterval < DefaultTimerInterval)
            {
                _timerInterval = DefaultTimerInterval;
            }

            _timer = new Timer { Interval = _timerInterval.TotalMilliseconds };
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false;
            _timer.Start();
        }

        /// <summary>
        /// Start to monitor metrics with specified timer interval
        /// </summary>
        /// <param name="interval">monioter timer interval</param>
        protected void Start(TimeSpan interval)
        {
            SetInterval(interval);

            Start();
        }

        /// <summary>
        /// Stop to monitor metrics and release the timer
        /// </summary>
        protected void Stop()
        {
            Dispose();
        }

        /// <summary>
        /// Get the list of Metrics and publish them
        /// </summary>
        /// <returns></returns>
        protected abstract void DoMonitor();

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                DoMonitor();
            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException) throw;
            }
            finally
            {
                // Determine the next timer interval compensating for the elapsed execution time.
                // elapsedTime = DateTime.Now - e.SignalTime;
                var interval = (_timerInterval - (DateTime.Now - e.SignalTime)).TotalMilliseconds;

                if (interval < DefaultTimerInterval.TotalMilliseconds/2)
                {
                    interval = DefaultTimerInterval.TotalMilliseconds/2;
                }

                _timer.Interval = interval;
                _timer.Start();
            }
        }

        /// <summary>
        /// Dispose the object and stop the timer
        /// </summary>
        public void Dispose()
        {
            var timer = _timer;
            _timer = null;

            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}
