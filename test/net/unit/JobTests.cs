//-----------------------------------------------------------------------
// <copyright file="JobTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
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

		 [TestMethod]
		 [ExpectedException(typeof(InvalidOperationException))]
		 public void CancelJobEmptyId()
		 {
			 var data = new JobData();
			 data.Cancel();
		 }

		 [TestMethod]
		 public void CancelJob()
		 {
			 var dataContextMock = new Mock<IMediaDataServiceContext>();

			 var fakeResponse = new string[] { "" };

			 var data = (JobData)_mediaContext.Jobs.First();

			 _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			 data.SetMediaContext(_mediaContext);

			 dataContextMock.Setup((ctxt) => ctxt
				 .ExecuteAsync<string>(It.IsAny<Uri>(), data))
				 .Returns(() => Task.Factory.StartNew<IEnumerable<string>>((d) =>
				 {
					 return null;
				 }, data));

			 data.Cancel();

			 Uri uri = new Uri(
			   string.Format(CultureInfo.InvariantCulture, "/CancelJob?jobid='{0}'", data.Id),
			   UriKind.Relative);

			 dataContextMock.Verify((ctxt) => ctxt.ExecuteAsync<string>(uri, data), Times.Once);
		 }

		 [TestMethod]
		 public void GetExecutionProgressTask()
		 {
			 _mediaContext.Jobs.Create("Name");
			 var remoteJob = _mediaContext.Jobs.First();
			 ((JobData)remoteJob).State = (int)JobState.Canceled;

			 var data = new JobData { Id = "1" };
			 bool stateChanged = false;
			 data.StateChanged += (object sender, JobStateChangedEventArgs e) => stateChanged = true;

			 data.SetMediaContext(_mediaContext);

			 Task t = data.GetExecutionProgressTask(CancellationToken.None);

			 Thread.Sleep(1000);

			 data.Id = remoteJob.Id;

			 t.Wait();

			 Assert.IsTrue(stateChanged);
		 }

		 [TestMethod]
		 public void SaveAsTemplateAsync()
		 {
			 var mediaproc = _mediaContext.MediaProcessors.FirstOrDefault();
			 var job = _mediaContext.Jobs.Create("Name");
			 var asset = _mediaContext.Assets.FirstOrDefault();
			 Assert.IsNotNull(asset);
			 var task = job.Tasks.AddNew("Task", mediaproc, Guid.NewGuid().ToString(), TaskOptions.None);
			 task.TaskBody = "<taskBody taskTemplateId='1'/>";
			 task.InputAssets.Add(asset);
			 task.OutputAssets.AddNew("OutPut");
			 var actual = job.SaveAsTemplate("template");

			 Assert.AreEqual(1, actual.NumberofInputAssets);
			 Assert.AreEqual("template", actual.Name);

			 var template =_mediaContext.JobTemplates.First();

			 var taskTemplates = template.TaskTemplates;
			 Assert.AreEqual(1, taskTemplates.Count);
		 }

		 #region test retry logic
		 [TestMethod]
		 public void TestJobUpdateRetry()
		 {
			 var dataContextMock = new Mock<IMediaDataServiceContext>();

			 int exceptionCount = 2;

			 var job = new JobData { Name = "testData", Id = "id:someid" };
			 var fakeResponse = new TestMediaDataServiceResponse { AsyncState = job };
			 var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			 dataContextMock.Setup((ctxt) => ctxt.AttachTo("Jobs", job));
			 dataContextMock.Setup((ctxt) => ctxt.DeleteObject(job));

			 dataContextMock.Setup((ctxt) => ctxt
				 .SaveChangesAsync(job))
				 .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
				 {
					 if (--exceptionCount > 0) throw fakeException;
					 return fakeResponse;
				 }));

			 _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			 job.SetMediaContext(_mediaContext);

			 job.Update();

			 Assert.AreEqual(0, exceptionCount);
		 }

		 [TestMethod]
		 public void TestJobDeleteRetry()
		 {
			 var dataContextMock = new Mock<IMediaDataServiceContext>();

			 int exceptionCount = 2;

			 var job = new JobData { Name = "testData", Id = "id:someid" };
			 var fakeResponse = new TestMediaDataServiceResponse { AsyncState = job };
			 var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			 dataContextMock.Setup((ctxt) => ctxt.AttachTo("Jobs", job));
			 dataContextMock.Setup((ctxt) => ctxt.DeleteObject(job));

			 dataContextMock.Setup((ctxt) => ctxt
				 .SaveChangesAsync(job))
				 .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
				 {
					 if (--exceptionCount > 0) throw fakeException;
					 return fakeResponse;
				 }));

			 _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			 job.SetMediaContext(_mediaContext);

			 job.Delete();

			 Assert.AreEqual(0, exceptionCount);
		 }

		 [TestMethod]
		 public void TestJobSubmitRetry()
		 {
			 var dataContextMock = new Mock<IMediaDataServiceContext>();

			 int exceptionCount = 2;

			 var job = new JobData { Name = "testData", TemplateId = "id:sometemplate" };
			 var fakeResponse = new TestMediaDataServiceResponse { AsyncState = job };
			 var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			 dataContextMock.Setup((ctxt) => ctxt.AttachTo("Jobs", job));
			 dataContextMock.Setup((ctxt) => ctxt.DeleteObject(job));

			 dataContextMock.Setup((ctxt) => ctxt
				 .SaveChangesAsync(SaveChangesOptions.Batch, job))
				 .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
				 {
					 if (--exceptionCount > 0) throw fakeException;
					 job.Id = Guid.NewGuid().ToString();
					 return fakeResponse;
				 }));

			 _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			 job.SetMediaContext(_mediaContext);

			 job.Submit();

			 Assert.AreEqual(0, exceptionCount);
		 }

		 [TestMethod]
		 [TestCategory("DailyBvtRun")]
		 public void TestJobGetContentKeysRetry()
		 {
			 var data = new JobData { Name = "testData", Id = "testId" };

			 var dataContextMock = TestMediaServicesClassFactory.CreateLoadPropertyMockConnectionClosed(2, data);

			 _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			 data.SetMediaContext(_mediaContext);

			 var actual = ((IJob)data).InputMediaAssets;

			 dataContextMock.Verify((ctxt) => ctxt.LoadProperty(data, "InputMediaAssets"), Times.Exactly(2));
		 }
		 #endregion test retry logic
	}
}