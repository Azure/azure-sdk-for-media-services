//-----------------------------------------------------------------------
// <copyright file="CacheTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void CacheTrivial()
        {
            var target = new Cache<string>();
            var actual = target.GetOrAdd("k", () => "v1", () => DateTime.UtcNow);
            Assert.AreEqual("v1", actual);
        }

        [TestMethod]
        public void CacheBeforeExpiration()
        {
            var target = new Cache<string>();
            target.GetOrAdd("k", () => "v1", () => DateTime.UtcNow.AddMinutes(1));
            var actual = target.GetOrAdd("k", () => { throw new InvalidOperationException(); }, () => { throw new InvalidOperationException(); });
            Assert.AreEqual("v1", actual);
        }

        [TestMethod]
        public void CacheAfterExpiration()
        {
            var target = new Cache<string>();
            target.GetOrAdd("k", () => "v1", () => DateTime.UtcNow.AddMilliseconds(100));
            Thread.Sleep(1000);
            var actual = target.GetOrAdd("k", () => "v2", () => DateTime.UtcNow.AddMilliseconds(100));
            Assert.AreEqual("v2", actual);
        }
    }
}
