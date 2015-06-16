
//-----------------------------------------------------------------------
// <copyright file="MemoryManager.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class MemoryManager
    {
        private readonly MemoryPool _memoryPool;

        private int _buffersInUse;

        public MemoryManager(int bufferSize)
            : this(bufferSize, Environment.Is64BitProcess ? 0x40000000L : 0x4000000L)
        {
        }

        public MemoryManager(int bufferSize, long capacity)
        {
            long num = capacity / ((long)bufferSize);
            int cellsCount = (int)Math.Min(0x2000L, num);
            _memoryPool = new MemoryPool(cellsCount, bufferSize);
        }

        public void ReleaseBuffer(byte[] buffer)
        {
            Interlocked.Decrement(ref _buffersInUse);
            _memoryPool.AddBuffer(buffer);
        }

        public void ReleaseUnusedBuffers()
        {
            _memoryPool.ReleaseUnusedBuffers();
        }

        public byte[] RequireBuffer()
        {
            byte[] buffer = _memoryPool.GetBuffer();
            if (buffer != null)
            {
                Interlocked.Increment(ref _buffersInUse);
            }
            return buffer;
        }

        private class MemoryCell
        {
            private byte[] _buffer;

            public MemoryCell(int size)
            {
                _buffer = new byte[size];
            }

            public byte[] Buffer
            {
                get
                {
                    return _buffer;
                }
            }

            public MemoryManager.MemoryCell NextCell { get; set; }
        }

        private class MemoryPool
        {
            private int _allocatedCells;
            private int _availableCells;
            private readonly ConcurrentDictionary<byte[], MemoryManager.MemoryCell> _cellsInUse;
            private MemoryManager.MemoryCell _cellsListHeadCell;
            private readonly object _cellsListLock;

            public MemoryPool(int cellsCount, int bufferSize)
            {
                BufferSize = bufferSize;
                _availableCells = cellsCount;
                _allocatedCells = 0;
                _cellsListLock = new object();
                _cellsListHeadCell = null;
                _cellsInUse = new ConcurrentDictionary<byte[], MemoryManager.MemoryCell>();
            }

            public int BufferSize { get; set; }

            public void AddBuffer(byte[] buffer)
            {
                MemoryManager.MemoryCell cell;
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (buffer.Length != BufferSize)
                {
                    throw new ArgumentException("Buffer is not of right size. Expected size " + BufferSize, "buffer");
                }
                if (!_cellsInUse.TryRemove(buffer, out cell))
                {
                    throw new ArgumentException("Buffer not created by this pool", "buffer");
                }
                lock (_cellsListLock)
                {
                    cell.NextCell = _cellsListHeadCell;
                    _cellsListHeadCell = cell;
                    _availableCells++;
                }
            }

            public void ReleaseUnusedBuffers()
            {
                lock (_cellsListLock)
                {
                    while (_cellsListHeadCell != null)
                    {
                        MemoryManager.MemoryCell cell = _cellsListHeadCell;
                        _cellsListHeadCell = _cellsListHeadCell.NextCell;
                        cell.NextCell = null;
                    }
                }
            }

            public byte[] GetBuffer()
            {
                if (_availableCells > 0)
                {
                    MemoryManager.MemoryCell cellsListHeadCell = null;
                    lock (_cellsListLock)
                    {
                        if (_availableCells > 0)
                        {
                            if (_cellsListHeadCell != null)
                            {
                                cellsListHeadCell = _cellsListHeadCell;
                                _cellsListHeadCell = cellsListHeadCell.NextCell;
                                cellsListHeadCell.NextCell = null;
                            }
                            else
                            {
                                cellsListHeadCell = new MemoryManager.MemoryCell(BufferSize);
                                _allocatedCells++;
                            }
                            _availableCells--;
                        }
                    }
                    if (cellsListHeadCell != null)
                    {
                        _cellsInUse.TryAdd(cellsListHeadCell.Buffer, cellsListHeadCell);
                        return cellsListHeadCell.Buffer;
                    }
                }
                return null;
            }
        }

    }

}
