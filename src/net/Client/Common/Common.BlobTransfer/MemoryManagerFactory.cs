//-----------------------------------------------------------------------
// <copyright file="MemoryManagerFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Concurrent;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class MemoryManagerFactory
    {
        private static ConcurrentDictionary<int, MemoryManager> _memoryManagers
            = new ConcurrentDictionary<int, MemoryManager>();

        public MemoryManager GetMemoryManager(int blockSize)
        {
            return _memoryManagers.GetOrAdd(
                blockSize,
                (size) => new MemoryManager(size));
        }
    }
}
