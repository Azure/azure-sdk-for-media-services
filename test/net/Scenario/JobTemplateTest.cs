using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;
using System.Net;
using System.Data.Services.Client;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    /// <summary>
    ///This is a test class for JobTemplateDataTest and is intended
    ///to contain all JobTemplateDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class JobTemplateTest
    {
        private CloudMediaContext _mediaContext;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        /// <summary>
        ///A test for SaveAsync
        ///</summary>
        [TestMethod()]
        [Priority(0)]
        [TestCategory("DailyBvtRun")]
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
        [Priority(0)]
        [TestCategory("DailyBvtRun")]
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
