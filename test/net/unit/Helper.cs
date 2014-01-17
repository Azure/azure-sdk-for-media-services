using System;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    public class Helper
    {
        public static CloudMediaContext GetMediaDataServiceContextForUnitTests()
        {
            CloudMediaContext mediaContext = new TestCloudMediaContext(new Uri("http://contoso.com"), new MediaServicesCredentials("", ""));
            TestCloudMediaDataContext testCloudMediaDataContext = new TestCloudMediaDataContext(mediaContext);
            mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(testCloudMediaDataContext);
            testCloudMediaDataContext.InitilizeStubData();
            return mediaContext;
        }
    }
}