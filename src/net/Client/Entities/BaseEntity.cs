//-----------------------------------------------------------------------
// <copyright file="IMediaContextContainer.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public abstract class BaseEntity : IMediaContextContainer
    {
        private MediaContextBase _mediaContextBase;

        public virtual void SetMediaContext(MediaContextBase value)
        {
            _mediaContextBase = value;
        }

        public virtual MediaContextBase GetMediaContext()
        {
            return _mediaContextBase;
        }

        protected void LoadProperty(IMediaDataServiceContext dataContext, string propertyName)
        {
            LoadProperty(dataContext, this, propertyName);
        }

        protected void LoadProperty(IMediaDataServiceContext dataContext, BaseEntity entity, string propertyName)
        {
            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);
            retryPolicy.ExecuteAction(() => dataContext.LoadProperty(entity, propertyName));
        }
    }

    public abstract class BaseEntity<T> : BaseEntity // todo: remove this class in a separate check-in. It will require a change to all entities.
    {
    }
}
