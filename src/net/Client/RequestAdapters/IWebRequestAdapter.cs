using System.Data.Services.Client;
using System.Net;
namespace Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters
{
    public interface IWebRequestAdapter
    {
        void AddClientRequestId(WebRequest request);
    }
}
