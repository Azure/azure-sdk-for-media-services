using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    class TestStreamingEndpointData : StreamingEndpointData
    {
        public TestStreamingEndpointData(StreamingEndpointCreationOptions options) : base(options)
        {
        }

        internal override void Refresh()
        {
        }
    }
}
