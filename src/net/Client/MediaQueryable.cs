//-----------------------------------------------------------------------
// <copyright file="MediaQueryable.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class MediaQueryable<TIinterface, TData> : IOrderedQueryable<TIinterface>
    {
        private IQueryable<TIinterface> _inner;

        public MediaQueryable(IQueryable<TIinterface> inner) : this(inner, null) {}

        public MediaQueryable(IQueryable<TIinterface> inner, MediaRetryPolicy queryRetryPolicy)
        {
            _inner = inner;
            Provider = new MediaQueryProvider<TData>(_inner.Provider, queryRetryPolicy);
        }

        #region IEnumerable

        public IEnumerator<TIinterface> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        public Type ElementType
        {
            get { return typeof(TIinterface); }
        }

        public Expression Expression
        {
            get 
            { 
                return _inner.Expression; 
            }
        }

        public IQueryProvider Provider { get; private set; }

        #endregion
    }
}
