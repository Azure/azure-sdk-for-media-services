//-----------------------------------------------------------------------
// <copyright file="TestCloudMediaDataContext.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Common
{
    public class TestCloudMediaDataContext : IMediaDataServiceContext,IRetryPolicyAdapter
    {
        private readonly MediaContextBase _mediaContextBase;
        private readonly Dictionary<Type, string> _entitySetMappings = new Dictionary<Type, string>();
        object _lock = new object();

        //Tracking all pending changes.
        //Please note that none committed changes are returned by queries
        private readonly Dictionary<string, object> _persistedChanges = new Dictionary<string, object>();
        private readonly Dictionary<string, object> _pendingChanges = new Dictionary<string, object>();
        private int _delaymilliseconds;

        public TestCloudMediaDataContext(MediaContextBase mediaContextBase)
        {
            _mediaContextBase = mediaContextBase;
        }

        public TestCloudMediaDataContext(MediaContextBase mediaContextBase, int delaymilliseconds)
        {
            _mediaContextBase = mediaContextBase;
            this._delaymilliseconds = delaymilliseconds;
        }

        public void InitilizeStubData()
        {
            string assetId = "nb:Id:" + Guid.NewGuid();

            StorageAccountData storageAccountData = new StorageAccountData
            {
                IsDefault = true,
                Name = "test storage",
            };

            _persistedChanges.Add(StorageAccountBaseCollection.EntitySet,
                new List<StorageAccountData>
                {
                    storageAccountData
                });

            AssetData assetData = new AssetData()
            {
                Id = assetId,
                AlternateId = String.Empty,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Name = "Mock Asset",
                Options = (int) AssetCreationOptions.None,
            };
            assetData.SetMediaContext(_mediaContextBase);
            _persistedChanges.Add(AssetCollection.AssetSet,
                new List<AssetData>
                {
                    assetData
                });
            string accessPolicyId = Guid.NewGuid().ToString();

            AccessPolicyData accessPolicyData = new AccessPolicyData()
            {
                Id = accessPolicyId,
                Name = "Mock AccessPolicy",
                Created = DateTime.UtcNow.AddDays(-1),
                DurationInMinutes = 10000,
                LastModified = DateTime.UtcNow,
                Permissions = (int) AccessPermissions.Read
            };
            accessPolicyData.SetMediaContext(_mediaContextBase);
            _persistedChanges.Add(AccessPolicyBaseCollection.AccessPolicySet,
                new List<AccessPolicyData>
                {
                    accessPolicyData
                });

            LocatorData locatorData = new LocatorData()
            {
                Id = Guid.NewGuid().ToString(),
                AssetId = assetId,
                Name = "Mock locator",
                AccessPolicyId = accessPolicyId,
                BaseUri = "http://"
            };

            locatorData.SetMediaContext(_mediaContextBase);
            _persistedChanges.Add(LocatorBaseCollection.LocatorSet,
                new List<LocatorData>
                {
                    locatorData
                });

            _persistedChanges.Add(AssetFileCollection.FileSet,
                new List<AssetFileData>
                {
                    new AssetFileData()
                    {
                        Id= Guid.NewGuid().ToString(),
                        Created = DateTime.UtcNow,
                        Name = "Mock File",
                        ParentAssetId = assetId
                    }
                });

            _persistedChanges.Add(MediaProcessorBaseCollection.MediaProcessorSet,
               new List<MediaProcessorData>
                {
                    new MediaProcessorData()
                    {
                        Id= Guid.NewGuid().ToString(),
                        Name = "Mock Processor",
                        Version = "1",
                        Vendor = "mock"
                    }
                });

            _persistedChanges.Add(JobBaseCollection.JobSet,
               new List<JobData>
                {
                    new JobData()
                    {
                        Id= Guid.NewGuid().ToString(),
                        Name = "Mock Job",
                    }
                });

            _persistedChanges.Add(AssetFilterBaseCollection.AssetFilterSet,
               new List<AssetFilterData>
                {
                    new AssetFilterData()
                    {
                        Id= Guid.NewGuid().ToString(),
                        Name = "Mock Asset Filter",
                    }
                });
        }

        public bool IgnoreResourceNotFoundException { get; set; }

        private IQueryable<T> CreateQuery<T>(string entitySetName)
        {
            
            if (!_persistedChanges.ContainsKey(entitySetName))
            {
                lock (_lock)
                {
                    _persistedChanges.Add(entitySetName,
                        new List<T>
                        {
                        });
                }
            }
            List<T> list = (List<T>)_persistedChanges[entitySetName];

            return list.AsQueryable();
        }

        public IQueryable<TIinterface> CreateQuery<TIinterface, TData>(string entitySetName)
        {
            IQueryable<TIinterface> inner = (IQueryable<TIinterface>)this.CreateQuery<TData>(entitySetName);
            var result = new MediaQueryable<TIinterface, TData>(inner, new MediaRetryPolicy(new QueryErrorDetectionStrategy(), new ExponentialBackoff()));
            return result;
        }

        public void AttachTo(string entitySetName, object entity)
        {
            const string methodName = "AttachTo";
            MethodInfo methodInfo = MakeMethodGeneric(entity, methodName);
            methodInfo.Invoke(this, new[] {entitySetName, entity});
        }

        public void AttachTo(string entitySetName, object entity, string etag)
        {
            throw new NotImplementedException();
        }

        public void DeleteObject(object entity)
        {
            const string methodName = "DeleteObject";
            MethodInfo methodInfo = MakeMethodGeneric(entity, methodName);
            methodInfo.Invoke(this, new[] {entity});
        }

        public IEnumerable<TElement> Execute<TElement>(Uri requestUri)
        {
            byte[] _knownGoodContentKey = {243, 220, 162, 177, 198, 142, 141, 81, 105, 142, 159, 49, 81, 69, 132, 217, 120, 15, 170, 6, 60, 59, 211, 247, 161, 12, 210, 74, 65, 6, 142, 205};
            
            if (requestUri.ToString().StartsWith("/RebindContentKey?"))
            {
                System.Collections.Specialized.NameValueCollection parameters = System.Web.HttpUtility.ParseQueryString(requestUri.ToString().Replace("/RebindContentKey?", String.Empty));
                string s = parameters["id"];
                if (s != null)
                {
                    var key = CreateQuery<ContentKeyData>(ContentKeyBaseCollection.ContentKeySet).Where(c => c.Id == s.Replace("'", String.Empty)).FirstOrDefault();
                    return new List<TElement>
                    {
                        (TElement) ((object) Convert.ToBase64String(_knownGoodContentKey))
                    };
                }
            }
            var cert = new X509Certificate2("UnitTest.pfx");
            string response = Convert.ToBase64String(cert.RawData);
            return new List<TElement>
            {
                (TElement) ((object) response)
            };
        }

        public OperationResponse Execute(Uri requestUri, string httpMethod, params OperationParameter[] operationParameters)
        {
            throw new NotImplementedException();
        }

        public void AddObject(string entitySetName, object entity)
        {
            MethodInfo methodInfo = this.GetType().GetMethods().Where(c => c.Name == "AddObject" && c.IsGenericMethod).First();
            methodInfo = methodInfo.MakeGenericMethod(new[] {entity.GetType()});
            methodInfo.Invoke(this, new[] {entitySetName, entity});
        }

        public QueryOperationResponse LoadProperty(object entity, string propertyName)
        {
            //Example of Load property simulation
            if (entity is AssetData)
            { 
                AssetData assetData = (AssetData)(entity);
                switch (propertyName)
                {
                    case "Locators":
                        assetData.Locators = new List<LocatorData>(CreateQuery<LocatorData>("Locators").Where(c=>c.AssetId ==c.AssetId ).ToList());
                        break;
                    default: break;
                }
            }
            if (entity is LocatorData)
            {
                LocatorData data = (LocatorData)(entity);
                switch (propertyName)
                {
                    case "Asset":
                        data.Asset =CreateQuery<AssetData>("Assets").Where(c => c.Id == data.AssetId).FirstOrDefault();
                        break;
                    case "AccessPolicy":
                        data.AccessPolicy= CreateQuery<AccessPolicyData>(AccessPolicyBaseCollection.AccessPolicySet).Where(c => c.Id == data.AccessPolicyId).FirstOrDefault();
                        break;
                    default: break;
                }
            }
            if (entity is JobTemplateData)
            {
                JobTemplateData data = (JobTemplateData)(entity);
                switch (propertyName)
                {
                    case "TaskTemplates":
                        data.TaskTemplates = CreateQuery<TaskTemplateData>("TaskTemplates").ToList();
                        break;
                    default: break;
                }
            }
            return null;
        }

        public void UpdateObject(object entity)
        {
        }

        public void SetLink(object source, string sourceProperty, object target)
        {
            var locator = source as LocatorData;
            var asset = target as AssetData;
            if (locator!=null && asset!=null)
            {
                asset.Locators.Add(locator);
                return;
            }

            var ingestAsset = source as IngestManifestAssetData;
            
            if (ingestAsset != null && asset != null)
            {
                ingestAsset.Asset = asset;
                return;
            }
        }

        public void AddLink(object source, string sourceProperty, object target)
        {
        }

        public IMediaDataServiceResponse SaveChanges()
        {
            return SaveChangesAsync(null).Result;
        }

        public void DeleteLink(object source, string sourceProperty, object target)
        {
            
        }

        public void AddRelatedObject(object source, string sourceProperty, object target)
        {
            MethodInfo methodInfo = this.GetType().GetMethods().Where(c => c.Name == "AddObject" && c.IsGenericMethod).First();
            methodInfo = methodInfo.MakeGenericMethod(new[] { target.GetType() });
            methodInfo.Invoke(this, new[] { sourceProperty, target });           
        }

        public Task<IEnumerable<T>> ExecuteAsync<T>(DataServiceQueryContinuation<T> continuation, object state)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ExecuteAsync<T>(Uri requestUri, object state)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResponse> ExecuteAsync(Uri requestUri, object state, string httpMethod)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ExecuteAsync<T>(Uri requestUri, string httpMethod, bool singleResult, params OperationParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public Task<DataServiceResponse> ExecuteBatchAsync(object state, params DataServiceRequest[] queries)
        {
            throw new NotImplementedException();
        }

        public Task<DataServiceStreamResponse> GetReadStreamAsync(object entity, DataServiceRequestArgs args, object state)
        {
            throw new NotImplementedException();
        }

        public Task<QueryOperationResponse> LoadPropertyAsync(object entity, string propertyName, object state)
        {
            throw new NotImplementedException();
        }

        public Task<QueryOperationResponse> LoadPropertyAsync(object entity, string propertyName, DataServiceQueryContinuation continuation, object state)
        {
            throw new NotImplementedException();
        }

        public Task<QueryOperationResponse> LoadPropertyAsync(object entity, string propertyName, Uri nextLinkUri, object state)
        {
            throw new NotImplementedException();
        }

        private Func<object, IMediaDataServiceResponse> SaveChangesFunc(object state)
        {
            return (object c) =>
            {
                if (_delaymilliseconds > 0)
                {
                    Thread.Sleep(_delaymilliseconds);
                }
                lock (_lock)
                {
                    foreach (var pendingChange in _pendingChanges)
                    {
                        if (_persistedChanges.ContainsKey(pendingChange.Key))
                        {

                            var addRamgeMethodInfo = _persistedChanges[pendingChange.Key].GetType().GetMethods().Where(m => m.Name == "AddRange").FirstOrDefault();
                            if (addRamgeMethodInfo != null)
                            {
                                addRamgeMethodInfo.Invoke(_persistedChanges[pendingChange.Key], new[] {pendingChange.Value});
                            }

                        }
                        else
                        {
                            _persistedChanges.Add(pendingChange.Key, pendingChange.Value);
                        }
                    }
                    _pendingChanges.Clear();
                }
                IMediaDataServiceResponse response = new TestMediaDataServiceResponse();
                if (state != null)
                {
                    response.AsyncState = state;
                    if (state.GetType().GetProperty("Id") != null)
                    {
                        state.GetType().InvokeMember("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, state, new[] {"nb:kid:UUID:" + Guid.NewGuid()});
                    }
                    if (state is IMediaContextContainer)
                    {
                        ((IMediaContextContainer) state).SetMediaContext(_mediaContextBase);
                    }

                    if (state is LocatorData)
                    {
                        ((LocatorData) state).BaseUri = "http://contoso.com/" + Guid.NewGuid().ToString();
                        ((LocatorData) state).Path = "http://contoso.com/" + Guid.NewGuid().ToString();
                        ((LocatorData) state).ContentAccessComponent = Guid.NewGuid().ToString();
                    }
                    if (state is AssetData)
                    {
                        ((AssetData) state).Uri = "http://contoso.com/" + Guid.NewGuid().ToString();
                    }
                }
                return response;
            };
        }
        public Task<IMediaDataServiceResponse> SaveChangesAsync(object state)
        {
            return Task.Factory.StartNew( SaveChangesFunc(state),
                state,
                CancellationToken.None);
        }

        public Task<IMediaDataServiceResponse> SaveChangesAsync(SaveChangesOptions options, object state)
        {
            return SaveChangesAsync(state);
        }

        public Task<IMediaDataServiceResponse> SaveChangesAsync(SaveChangesOptions options, object state, CancellationToken token)
        {
            return Task.Factory.StartNew(SaveChangesFunc(state),
                state,
                token);
        }

        private MethodInfo MakeMethodGeneric(object entity, string methodName)
        {
            MethodInfo methodInfo = this.GetType().GetMethods().Where(c => c.Name == methodName && c.IsGenericMethod).First();
            methodInfo = methodInfo.MakeGenericMethod(new[] {entity.GetType()});
            return methodInfo;
        }

        public void AttachTo<T>(string entitySetName, T entity)
        {
            if (!this._entitySetMappings.ContainsKey(typeof(T)))
            {
                _entitySetMappings.Add(typeof(T), entitySetName);
                if (!_pendingChanges.ContainsKey(entitySetName))
                {
                    _pendingChanges.Add(entitySetName,
                        new List<T>
                    {
                        entity
                    });
                }
            }
            
        }

        public void DeleteObject<T>(T entity)
        { 
            if (!this._entitySetMappings.ContainsKey(typeof(T)))
            {
                throw new InvalidDataException(string.Format("There is no enity mapping configured for type {0}",typeof(T).Name));
            }
            string entitySetName = _entitySetMappings[typeof (T)];

            if (_persistedChanges.ContainsKey(entitySetName))
            {
                ((List<T>) _persistedChanges[entitySetName]).Remove(entity);
            }
        }

        public void AddObject<T>(string entitySetName, T entity)
        {
            //Since we are persiting objects here and not in SaveChanges, we need to have delay in order to test cancellation token
            if (_delaymilliseconds > 0)
            {
                Thread.Sleep(_delaymilliseconds);
            }

            if (!this._entitySetMappings.ContainsKey(typeof (T)))
            {
                lock (_lock)
                {
                    if (!this._entitySetMappings.ContainsKey(typeof (T)))
                    {
                        _entitySetMappings.Add(typeof (T), entitySetName);
                    }
                }
            }

            if (!_pendingChanges.ContainsKey(entitySetName))
            {
                
                if (entity is IMediaContextContainer)
                {
                    ((IMediaContextContainer)entity).SetMediaContext(_mediaContextBase);
                }
                lock (_lock)
                {
                    if (!_pendingChanges.ContainsKey(entitySetName))
                    {
                        _pendingChanges.Add(entitySetName,
                            new List<T>
                            {
                                entity
                            });
                    }
                }
            }
            else
            {
                List<T> list = _pendingChanges[entitySetName] as List<T>;
                list.Add(entity);
            }
        }

        public Func<Task<TResult>> AdaptExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
        {
            return taskFunc;
        }

        public Func<Task> AdaptExecuteAsync(Func<Task> taskFunc)
        {
            return taskFunc;
        }

        public Func<TResult> AdaptExecuteAction<TResult>(Func<TResult> func)
        {
            return func;
        }
    }
}