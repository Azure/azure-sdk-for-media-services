using System.Data.Services.Client;

namespace Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters
{
    public interface IDataServiceContextAdapter
    {
        void Adapt(DataServiceContext context);
    }
}
