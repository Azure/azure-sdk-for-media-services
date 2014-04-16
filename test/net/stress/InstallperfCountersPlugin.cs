//-----------------------------------------------------------------------
// <copyright file="InstallperfCountersPlugin.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.LoadTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Stress
{
    /// <summary>
    /// InstallperfCountersPlugin creates perf counter category before stress run.
    /// </summary>
    public class InstallperfCountersPlugin : ILoadTestPlugin
    {
        private LoadTest _myLoadTest;

        public void Initialize(LoadTest loadTest)
        {
            _myLoadTest = loadTest;
            _myLoadTest.LoadTestStarting += new EventHandler(LoadTestStarting);
        }

        private void LoadTestStarting(object sender, EventArgs e)
        {
            // Delete the category if already exists   
            if (PerformanceCounterCategory.Exists("AMSStressCounterSet"))
            {

                PerformanceCounterCategory.Delete("AMSStressCounterSet");
            }

            CounterCreationDataCollection counters = new CounterCreationDataCollection();

                // 1. counter for counting totals: PerformanceCounterType.NumberOfItems32
                CounterCreationData totalOps = new CounterCreationData();
                totalOps.CounterName = "# operations executed";
                totalOps.CounterHelp = "Total number of operations executed";
                totalOps.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(totalOps);

                // 2. counter for counting operations per second:
                //        PerformanceCounterType.RateOfCountsPerSecond32
                CounterCreationData opsPerSecond = new CounterCreationData();
                opsPerSecond.CounterName = "# operations / sec";
                opsPerSecond.CounterHelp = "Number of operations executed per second";
                opsPerSecond.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(opsPerSecond);

                // create new category with the counters above
                PerformanceCounterCategory.Create("AMSStressCounterSet", "KeyDelivery Stress Counters", PerformanceCounterCategoryType.SingleInstance, counters);
        }

    }
}
