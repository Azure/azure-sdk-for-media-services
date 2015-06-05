using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit.Live
{
    class TestRestEntity : RestEntity<StreamingEndpointData>
    {
        public TestRestEntity(MediaContextBase context)
        {
            SetMediaContext(context);
        }

        public void ExecuteActionAsyncTest()
        {
            ExecuteActionAsync(new Uri("http://whatever"), TimeSpan.FromMilliseconds(1)).Wait();
        }

        public void RefreshTest()
        {
            Refresh();
        }

        public IOperation SendOperationTest()
        {
            return SendOperation(new Uri("http://whatever"));
        }

        protected override string EntitySetName
        {
            get { return "StreamingEndpoints"; }
        }
    }
}