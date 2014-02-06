//-----------------------------------------------------------------------
// <copyright file="MediaQueryProvider.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class MediaQueryProvider<TData> : IQueryProvider
    {
        private IQueryProvider _inner;
		private MediaRetryPolicy _queryRetryPolicy;

		public MediaQueryProvider(IQueryProvider inner, MediaRetryPolicy queryRetryPolicy)
        {
            _inner = inner;
			_queryRetryPolicy = queryRetryPolicy;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (!typeof(TElement).IsAssignableFrom(typeof(TData)))
            {
                var innerResult = _inner.CreateQuery<TElement>(expression);
                return innerResult;
            }

            var result = (IQueryable<TElement>)_inner.CreateQuery<TElement>(expression).Cast<TData>();
            return (IQueryable<TElement>)new MediaQueryable<TElement, TData>(result);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return _inner.CreateQuery(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
			if(_queryRetryPolicy == null)
			{
				return _inner.Execute<TResult>(expression);
			}
			else
			{
				return _queryRetryPolicy.ExecuteAction(() => _inner.Execute<TResult>(expression));
			}
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }
    }
}
