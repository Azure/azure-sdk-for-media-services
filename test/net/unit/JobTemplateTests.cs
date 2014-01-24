//-----------------------------------------------------------------------
// <copyright file="JobTemplateTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
	public class JobTemplateTests
	{
		private CloudMediaContext _mediaContext;
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public void SetupTest()
		{
			_mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
		}

		[TestMethod]
		public void JobTemplateQueryable()
		{
			IJobTemplate template = _mediaContext.JobTemplates.FirstOrDefault();
			var templates = _mediaContext.Jobs.Take(5);
		}

		[TestMethod]
		public void SaveTemplate()
		{
			var data = new JobTemplateData
			{
				JobTemplateBodyCopied = "taskTemplateId=\"nb:ttid:UUID:1\"",
				TaskTemplates = new List<TaskTemplateData> 
				 { 
					 new TaskTemplateData {}
				 }
			};
			data.SetMediaContext(_mediaContext);
			data.Save();

			var newTaskTemplateId = data.TaskTemplates.Single().Id.Substring("nb:ttid:UUID:".Length);
			Guid.Parse(newTaskTemplateId);
		}

		[TestMethod]
		public void CopyTemplate()
		{
			var data = new JobTemplateData
			{
				JobTemplateBodyCopied = "taskTemplateId=\"nb:ttid:UUID:1\"",
				Name = Guid.NewGuid().ToString(),
				TaskTemplates = new List<TaskTemplateData> 
				 { 
					 new TaskTemplateData {}
				 },
			};
			data.SetMediaContext(_mediaContext);
			var copy = data.Copy();

			Assert.AreEqual(data.Name, copy.Name);
		}

		[TestMethod]
		public void DeleteTemplate()
		{
			var dataContextMock = new Mock<IMediaDataServiceContext>();

			var fakeResponse = new string[] { "" };

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			var data = new JobTemplateData
			{
				JobTemplateBodyCopied = "taskTemplateId=\"nb:ttid:UUID:1\"",
				Name = Guid.NewGuid().ToString(),
				Id = "1",
				TaskTemplates = new List<TaskTemplateData> 
				 { 
					 new TaskTemplateData {}
				 },
			};

			dataContextMock.Setup((ctxt) => ctxt
				.SaveChangesAsync(data))
				.Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>((d) =>
				{
					return null;
				}, data));

			data.SetMediaContext(_mediaContext);

			data.Delete();

			dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Once);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DeleteUnsavedTemplate()
		{
			var data = new JobTemplateData { };
			data.Delete();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SaveSavedTemplate()
		{
			var data = new JobTemplateData { Id = "1" };
			data.Save();
		}

		[TestMethod]
		public void TemplateType()
		{
			var data = new JobTemplateData
			{
				TemplateType = (int)JobTemplateType.AccountLevel
			};

			IJobTemplate iData = (IJobTemplate)data;

			Assert.AreEqual(JobTemplateType.AccountLevel, iData.TemplateType);

			iData.TemplateType = JobTemplateType.SystemLevel;

			Assert.AreEqual((int)JobTemplateType.SystemLevel, data.TemplateType);
		}

		/// <summary>
		///A test for SaveAsync
		///</summary>
		[TestMethod()]
		public void JobTemplateTestSaveAsyncRetry()
		{
			JobTemplateData data = new JobTemplateData { JobTemplateBodyCopied = "" };

			var fakeResponse = new TestMediaDataServiceResponse { AsyncState = data };
			var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			var dataContextMock = new Mock<IMediaDataServiceContext>();

			dataContextMock.Setup((ctxt) => ctxt.AttachTo("Jobs", data));
			dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

			int saveChangesExceptionCount = 2;

			dataContextMock.Setup((ctxt) => ctxt
				.SaveChangesAsync(SaveChangesOptions.Batch, data))
				.Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
				{
					if (--saveChangesExceptionCount > 0) throw fakeException;
					return fakeResponse;
				}));

			int loadPropertiesExceptionCount = 2;
			dataContextMock.Setup((ctxt) => ctxt
				.LoadProperty(data, It.IsAny<string>()))
				.Returns(() =>
				{
					if (--loadPropertiesExceptionCount > 0) throw fakeException;
					return null;
				});

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			data.SetMediaContext(_mediaContext);

			data.SaveAsync().Wait();

			dataContextMock.Verify((ctxt) => ctxt.LoadProperty(data, "TaskTemplates"), Times.Exactly(2));
			Assert.AreEqual(0, saveChangesExceptionCount);
			Assert.AreEqual(0, loadPropertiesExceptionCount);
		}

		[TestMethod]
		public void JobTemplateTestDeleteRetry()
		{
			JobTemplateData data = new JobTemplateData { JobTemplateBodyCopied = "", Id = "fakeId" };

			var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

			var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

			dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeyAuthorizationPolicies", data));
			dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

			_mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

			data.SetMediaContext(_mediaContext);

			data.Delete();

			dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
		}
	}
}