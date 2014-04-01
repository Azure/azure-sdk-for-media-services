//-----------------------------------------------------------------------
// <copyright file="TestMediaServicesClassFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using System.Net;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Common
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

        /// <summary>
        /// Creates retry policy for saving changes in Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetSaveChangesRetryPolicy()
        {
            var retryPolicy = new MediaRetryPolicy(
                GetSaveChangesErrorDetectionStrategy(),
                retryCount: 5,
                minBackoff: TimeSpan.FromMilliseconds(10),
                maxBackoff: TimeSpan.FromMilliseconds(10000),
                deltaBackoff: TimeSpan.FromMilliseconds(50));

            return retryPolicy;
        }

        public override MediaErrorDetectionStrategy GetSaveChangesErrorDetectionStrategy()
        {
            return new WebRequestTransientErrorDetectionStrategy();
        }

        public override MediaErrorDetectionStrategy GetQueryErrorDetectionStrategy()
        {
            return new WebRequestTransientErrorDetectionStrategy();
        }

        public static Mock<IMediaDataServiceContext> CreateSaveChangesMock<T>(Exception fakeException, int failCount,  BaseEntity<T> returnedData)
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();
            var fakeResponse = new TestMediaDataServiceResponse { AsyncState = returnedData };
            int exceptionCount = failCount;

            dataContextMock.Setup((ctxt) => ctxt
                .SaveChangesAsync(It.IsAny<object>()))
                .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                }));

            dataContextMock.Setup((ctxt) => ctxt
                .SaveChanges())
                .Returns(() => 
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                });

            return dataContextMock;
        }

        public static Mock<IMediaDataServiceContext> CreateLoadPropertyMockConnectionClosed(int failCount, object entity)
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            int exceptionCount = failCount;

            dataContextMock.Setup((ctxt) => ctxt.AttachTo(It.IsAny<string>(), entity));

            dataContextMock.Setup((ctxt) => ctxt
                .LoadProperty(entity, It.IsAny<string>()))
                .Returns(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return null;
                });

            return dataContextMock;
        }

        private IMediaDataServiceContext _dataContext;

        public override BlobTransferClient GetBlobTransferClient()
        {
            Mock<BlobTransferClient> mock = new Mock<BlobTransferClient>(default(TimeSpan));
            mock.Setup(c => c.UploadBlob(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FileEncryption>(), It.IsAny<CancellationToken>(), It.IsAny<IRetryPolicy>())).Returns((Uri url,
            string localFile,
            string contentType,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            IRetryPolicy retryPolicy) => Task.Factory.StartNew(() =>
            {
                FileInfo info = new FileInfo(localFile);
                if (fileEncryption != null)
                {
                    lock (fileEncryption)
                    {
                        using (FileEncryptionTransform encryptor = fileEncryption.GetTransform(info.Name,0))
                        {
                            
                        }
                    }
                }
            }));
            mock.Setup(c => c.DownloadBlob(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<FileEncryption>(), It.IsAny<ulong>(), It.IsAny<CancellationToken>(), It.IsAny<IRetryPolicy>())).Returns(() => Task.Factory.StartNew(() => { }));
            mock.Setup(c => c.DownloadBlob(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<FileEncryption>(), It.IsAny<ulong>(),It.IsAny<CloudBlobClient>(), It.IsAny<CancellationToken>(), It.IsAny<IRetryPolicy>())).Returns(() => Task.Factory.StartNew(() => { }));
           return mock.Object;

        }
    }
}
