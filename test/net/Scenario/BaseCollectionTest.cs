//-----------------------------------------------------------------------
// <copyright file="BaseCollectionTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using System.Net;
using System.Threading.Tasks;
using System.Collections;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    class TestBaseCollection<T> : BaseCollection<T>
    {
        private IQueryable<T> _queryable;
        public TestBaseCollection(IQueryable<T> queryable, MediaContextBase context) : base(context)
        {
            _queryable = queryable;
        }

        protected override IQueryable<T> Queryable
        {
            get
            {
                return _queryable;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
    
    /// <summary>
    ///This is a test class for BaseCollectionTest and is intended
    ///to contain all BaseCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BaseCollectionTest
    {
        /// <summary>
        ///A test for GetEnumerator
        ///</summary>
        [TestMethod()]
        public void GetTypedEnumeratorTest()
        {
            var dataMock = new Mock<IQueryable<int>>();

            var mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            BaseCollection<int> target = new TestBaseCollection<int>(dataMock.Object, mediaContext);

            var dataContextMock = new Mock<IMediaDataServiceContext>();

            mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            int exceptionCount = 2;

            dataMock.Setup(q => q.GetEnumerator())
                .Returns(() => 
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return new[] {1,2,3}.AsQueryable().GetEnumerator();
                });

            IEnumerator<int> actual = target.GetEnumerator();

            Assert.IsTrue(MakeEnumerable(actual).SequenceEqual(new[] { 1, 2, 3 }));

            Assert.AreEqual(0, exceptionCount);
        }

        private static IEnumerable<T> MakeEnumerable<T>(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext()) yield return enumerator.Current;
        }

        /// <summary>
        ///A test for GetEnumerator
        ///</summary>
        [TestMethod()]
        public void GetUntypedEnumeratorTest()
        {
            var dataMock = new Mock<IQueryable<int>>();

            var mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            BaseCollection<int> target = new TestBaseCollection<int>(dataMock.Object, mediaContext);

            var dataContextMock = new Mock<IMediaDataServiceContext>();

            mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            int exceptionCount = 2;

            dataMock.Setup(q => q.GetEnumerator())
                .Returns(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return new[] { 1, 2, 3 }.AsQueryable().GetEnumerator();
                });

            IEnumerator actual = ((IEnumerable)target).GetEnumerator();

            Assert.AreEqual(0, exceptionCount);
        }
    }
}
