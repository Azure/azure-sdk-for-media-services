//-----------------------------------------------------------------------
// <copyright file="CriticalSection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Wrapper on Monitor that looks for out of order locks.
    /// Also can track what thread holds a lock.
    /// </summary>
    internal static class CriticalSection
    {
#if DEBUG
        /// <summary>
        /// Hold dependent lock information.
        /// </summary>
        internal class DependentLockInfo
        {
            /// <summary>
            /// Gets the lock info.
            /// </summary>
            public LockInfo LockInfo { get; private set; }

            /// <summary>
            /// Gets the call stacks.
            /// </summary>
            public List<StackTrace> CallStacks { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DependentLockInfo"/> class.
            /// </summary>
            /// <param name="lockInfo">The lock info.</param>
            public DependentLockInfo(LockInfo lockInfo)
            {
                this.LockInfo = lockInfo;
                this.CallStacks = new List<StackTrace>();
            }
        }

        /// <summary>
        /// Information on a single lock.
        /// </summary>
        internal class LockInfo
        {
            /// <summary>
            /// Lock object.
            /// </summary>
            private readonly WeakReference _lock;

            /// <summary>
            /// List of depentent locks.
            /// </summary>
            private readonly List<DependentLockInfo> _dependentLocks = new List<DependentLockInfo>();

            /// <summary>
            /// Current owning thread, -1 if no owner.
            /// </summary>
            private int _owningThread = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="LockInfo"/> class.
            /// </summary>
            /// <param name="obj">The obj.</param>
            public LockInfo(object obj)
            {
                this._lock = new WeakReference(obj);
            }

            /// <summary>
            /// Add a lock as a depenedtent of this lock.
            /// </summary>
            /// <param name="lockInfo">The lock information.</param>
            public void AddDependent(LockInfo lockInfo)
            {
#if !CAPTURE_STACK_TRACES
                if (this.IsDependent(lockInfo))
                {
                    return;
                }
#endif
                DependentLockInfo foundInfo = null;
                foreach (DependentLockInfo dependLockInfo in this._dependentLocks)
                {
                    if (dependLockInfo.LockInfo == lockInfo)
                    {
                        foundInfo = dependLockInfo;
                        break;
                    }
                }

                if (foundInfo == null)
                {
                    foundInfo = new DependentLockInfo(lockInfo);
                    this._dependentLocks.Add(foundInfo);
                }

#if CAPTURE_STACK_TRACES
				StackTrace callStack = new StackTrace(true);
				foreach (StackTrace currentTraces in foundInfo.CallStacks)
				{
					if (IsSameCallStack(callStack, currentTraces))
						return;
				}

				foundInfo.CallStacks.Add(callStack);
#endif
            }

#if CAPTURE_STACK_TRACES
			private static bool IsSameCallStack(StackTrace lhs, StackTrace rhs)
			{
				if (lhs.FrameCount != rhs.FrameCount)
                {
					return false;
                }

				for (int i = 0; i < lhs.FrameCount; i++)
				{
					StackFrame lhsFrame = lhs.GetFrame(i);
					StackFrame rhsFrame = rhs.GetFrame(i);

					if(lhsFrame.GetMethod() != rhsFrame.GetMethod())
                    {
						return false;
					}

                    if (lhsFrame.GetILOffset() != rhsFrame.GetILOffset())
                    {
						return false;
                    }
				}

				return true;
			}
#endif

            /// <summary>
            /// Is a lock a depenedent of this lock.
            /// </summary>
            /// <param name="lockInfo">The lock information.</param>
            /// <returns>True if the lock is dependent; false otherwise.</returns>
            public bool IsDependent(LockInfo lockInfo)
            {
                if (this == lockInfo)
                {
                    return true;
                }

                foreach (DependentLockInfo child in this._dependentLocks)
                {
                    if (child.LockInfo.IsDependent(lockInfo))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Gets or sets current owning thread.
            /// </summary>
            public int OwningThread
            {
                get
                {
                    return this._owningThread;
                }

                set
                {
                    this._owningThread = value;
                }
            }

            /// <summary>
            /// Gets the lock object.
            /// </summary>
            public object Lock
            {
                get
                {
                    return this._lock.Target;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is alive.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is alive; otherwise, <c>false</c>.
            /// </value>
            public bool IsAlive
            {
                get
                {
                    return this._lock.IsAlive;
                }
            }

            /// <summary>
            /// Kills a depend object, ihearting its dependents.
            /// </summary>
            /// <param name="lockInfo">The lock information.</param>
            public void KillDependent(LockInfo lockInfo)
            {
                int index = -1;
                for (int i = 0; i < this._dependentLocks.Count; i++)
                {
                    if (this._dependentLocks[i].LockInfo == lockInfo)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    return;
                }

                this._dependentLocks.RemoveAt(index);
                this._dependentLocks.AddRange(lockInfo._dependentLocks);
            }
        }

        /// <summary>
        /// Locks currently held by thread.
        /// </summary>
        [ThreadStatic]
        private static Stack<LockInfo> HeldLocks;

        /// <summary>
        /// List of all locks seen.
        /// </summary>
        internal static readonly List<LockInfo> RgLocks = new List<LockInfo>();

#endif
        /// <summary>
        /// Releases the lock on dispose.
        /// </summary>
        internal class ExitOnDispose : IDisposable
        {
#if DEBUG
            /// <summary>
            /// Lock object working with.
            /// </summary>
            private readonly LockInfo _lockInfo;

            /// <summary>
            /// Is this a recursive lock. If so don't reset owning thread.
            /// </summary>
            private readonly bool _recLock;

#endif
            /// <summary>
            /// Lock object.
            /// </summary>
            private readonly object _object;

#if DEBUG

            /// <summary>
            /// Initializes a new instance of the <see cref="ExitOnDispose"/> class.
            /// </summary>
            /// <param name="obj">The obj.</param>
            /// <param name="lockInfo">The lock info.</param>
            /// <param name="isRecLock">If set to <c>true</c> lock.</param>
            public ExitOnDispose(object obj, LockInfo lockInfo, bool isRecLock)
            {
                this._object = obj;

                this._lockInfo = lockInfo;
                this._recLock = isRecLock;
            }
#else
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="lockInfo"></param>
            /// <param name="fRecLock"></param>
            public ExitOnDispose(object obj)
            {
                this._object = obj;
            }
#endif

            /// <summary>
            /// Dispose, unlock the lock, reset state.
            /// </summary>
            public void Dispose()
            {
#if DEBUG
                if (!_recLock)
                {
                    _lockInfo.OwningThread = -1;
                }

                Debug.Assert(HeldLocks.Peek() == _lockInfo, "Can not dispose lock not owned.");
                HeldLocks.Pop();
#endif
                Monitor.Exit(_object);
                GC.SuppressFinalize(this);
            }
#if DEBUG

            /// <summary>
            /// Finalizes an instance of the <see cref="ExitOnDispose" /> class.
            /// </summary>
            ~ExitOnDispose()
            {
                // Destructor, should never be called. show use using statement
                Debug.Assert(false, "Use using keyword");
            }
#endif
        }

        /// <summary>
        /// Enter a critical section.
        /// </summary>
        /// <param name="obj">The object to use.</param>
        /// <returns>The disposable section holding the lock.</returns>
        public static IDisposable Enter(object obj)
        {
            bool isLockTaken = false;
            try
            {
                Monitor.Enter(obj, ref isLockTaken);

                // If we got this far shoud always be true.
                Debug.Assert(isLockTaken, "Lock is taken.");
#if DEBUG
                lock (RgLocks)
                {
                    if (HeldLocks == null)
                    {
                        HeldLocks = new Stack<LockInfo>();
                    }

                    LockInfo lockInfo = FindLock(obj);
                    LockInfo potentionDeadlock = null;
                    foreach (LockInfo currentLock in HeldLocks)
                    {
                        // Recursive lock ok.
                        if (currentLock == lockInfo)
                        {
                            potentionDeadlock = null;
                            continue;
                        }

                        if (currentLock.IsDependent(lockInfo))
                        {
                            potentionDeadlock = currentLock;
                        }
                    }

                    Debug.Assert(potentionDeadlock == null, "Potention Deadlock");
                    bool isRec = lockInfo.OwningThread != -1;
                    Debug.Assert(lockInfo.OwningThread == -1 || lockInfo.OwningThread == Thread.CurrentThread.ManagedThreadId, "Lock owned or managed thread does not own lock.");
                    lockInfo.OwningThread = Thread.CurrentThread.ManagedThreadId;
                    if (HeldLocks.Count != 0 && potentionDeadlock == null && !isRec)
                    {
                        lockInfo.AddDependent(HeldLocks.Peek());
                    }

                    HeldLocks.Push(lockInfo);
                    return new ExitOnDispose(obj, lockInfo, isRec);
                }
#else
				return new ExitOnDispose(obj);
#endif
            }
            catch
            {
                // If for some resons we messed up, release lock.
                if (isLockTaken)
                {
                    Monitor.Exit(obj);
                }

                throw;
            }
        }

#if DEBUG

        /// <summary>
        /// Find a lock.
        /// </summary>
        /// <param name="obj">The object to inspect.</param>
        /// <returns>The lock information for the object.</returns>
        private static LockInfo FindLock(object obj)
        {
            for (int i = RgLocks.Count - 1; i >= 0; i--)
            {
                if (!RgLocks[i].IsAlive)
                {
                    LockInfo infoRemove = RgLocks[i];
                    RgLocks.RemoveAt(i);
                    foreach (LockInfo info in RgLocks)
                    {
                        info.KillDependent(infoRemove);
                    }
                }
                else
                {
                    if (object.ReferenceEquals(RgLocks[i].Lock, obj))
                    {
                        return RgLocks[i];
                    }
                }
            }

            LockInfo lockInfo = new LockInfo(obj);
            RgLocks.Add(lockInfo);

            return lockInfo;
        }

        /// <summary>
        /// See if current thread holds the lock.
        /// </summary>
        /// <param name="obj">The object to inspect.</param>
        public static void CheckCurrentThreadHoldsLock(object obj)
        {
            bool isLockTaken = false;
            try
            {
                Monitor.TryEnter(obj, ref isLockTaken);
                // If we held lock, then we could always get it.
                if (!isLockTaken)
                {
                    Debug.Assert(false, "Current thread doesn't hold lock");
                }

                lock (RgLocks)
                {
                    LockInfo info = FindLock(obj);
                    Debug.Assert(info.OwningThread == Thread.CurrentThread.ManagedThreadId, "Current thread doesn't hold lock");
                }
            }
            finally
            {
                if (isLockTaken)
                {
                    Monitor.Exit(obj);
                }
            }
        }
#endif
    }
}
