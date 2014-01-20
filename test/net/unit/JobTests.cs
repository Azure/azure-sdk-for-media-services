//-----------------------------------------------------------------------
// <copyright file="TestMediaServicesClassFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.UnitTests
{
    [TestClass]
    public class JobTests
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void JobQueryable()
        {
            IJob job = _mediaContext.Jobs.FirstOrDefault();
            var jobs = _mediaContext.Jobs.Take(5);

        }

         [TestMethod]
        public void CreateJob()
         {
             var mediaproc = _mediaContext.MediaProcessors.FirstOrDefault();
             var job = _mediaContext.Jobs.Create("Name");
             var asset = _mediaContext.Assets.FirstOrDefault();
             Assert.IsNotNull(asset);
             var task = job.Tasks.AddNew("Task", mediaproc, Guid.NewGuid().ToString(), TaskOptions.None);
             task.InputAssets.Add(asset);
             task.OutputAssets.AddNew("OutPut");
             job.Submit();

         }

    }
}