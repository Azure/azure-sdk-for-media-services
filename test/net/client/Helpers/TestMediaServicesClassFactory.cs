using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers
{
    public class TestMediaServicesClassFactory : AzureMediaServicesClassFactory
    {
        public TestMediaServicesClassFactory(IMediaDataServiceContext dataContext)
        {
            _dataContext = dataContext;
        }

        public override IMediaDataServiceContext CreateDataServiceContext()
        {
            return _dataContext;
        }

        private IMediaDataServiceContext _dataContext;
    }
}
