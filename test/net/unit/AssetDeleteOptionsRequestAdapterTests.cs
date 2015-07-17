using System;
using System.Data.Services.Client;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class AssetDeleteOptionsRequestAdapterTests
    {
        [TestMethod]
        public void CheckParameterSetToTrue()
        {
            AssetDeleteOptionsRequestAdapter adapter = new AssetDeleteOptionsRequestAdapter(true);
            Uri uri = ExecuteAssetDeleteRequest(adapter);
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.Query.Contains("keepcontainer=true"));

        }

        [TestMethod]
        public void CheckParameterSetToFalse()
        {
            AssetDeleteOptionsRequestAdapter adapter = new AssetDeleteOptionsRequestAdapter(false);
            Uri uri = ExecuteAssetDeleteRequest(adapter);
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.Query.Contains("keepcontainer=false"));

        }

        private static Uri ExecuteAssetDeleteRequest( AssetDeleteOptionsRequestAdapter adapter)
        {
            Uri uri = null;
            var context = new DataServiceContext(new Uri("http://127.0.0.1/" + Guid.NewGuid().ToString()));
            bool sendingRequestCalled = false;
            context.SendingRequest2 += delegate(object o, SendingRequest2EventArgs args)
            {
                sendingRequestCalled = true;
                uri = args.RequestMessage.Url;
            };
            try
            {
                AssetData asset = new AssetData() {Id = Guid.NewGuid().ToString()};
                context.AttachTo("Assets", asset);
                context.DeleteObject(asset);
                adapter.Adapt(context);
                context.SaveChanges();
            }
            catch (DataServiceRequestException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            Assert.IsTrue(sendingRequestCalled);
            return uri;
        }
    }
}