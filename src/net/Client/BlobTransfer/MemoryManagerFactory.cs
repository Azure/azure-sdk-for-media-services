using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
