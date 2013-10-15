//-----------------------------------------------------------------------
// <copyright file="AssetCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

        public override MediaErrorDetectionStrategy GetSaveChangesErrorDetectionStrategy()
        {
            return new WebRequestTransientErrorDetectionStrategy();
        }

        public override MediaErrorDetectionStrategy GetQueryErrorDetectionStrategy()
        {
            return new WebRequestTransientErrorDetectionStrategy();
        }

        private IMediaDataServiceContext _dataContext;
    }
}
