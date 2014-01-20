//-----------------------------------------------------------------------
// <copyright file="QueryRetryTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.UnitTests
{
	class ThrowingProvider : IQueryProvider
	{
		bool _first = true;
		IQueryProvider _inner;

		public ThrowingProvider(IQueryProvider inner)
		{
			_inner = inner;
		}

		#region IQueryProvider Members

		public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
		{
			return _inner.CreateQuery<TElement>(expression);
		}

		public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
		{
			return _inner.CreateQuery(expression);
		}

		public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
		{
			if(_first)
			{
				_first = false;
				throw new TimeoutException("test timeout");
			}
			return _inner.Execute<TResult>(expression);
		}

		public object Execute(System.Linq.Expressions.Expression expression)
		{
			return _inner.Execute(expression);
		}

		#endregion
	}

	class ThrowingQueryable : IQueryable<string>
	{
		IQueryable<string> _inner = new[] { "first" }.AsQueryable();

		public IQueryable<string> Inner { get { return _inner; } }

		#region IEnumerable<string> Members

		public IEnumerator<string> GetEnumerator()
		{
			return _inner.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IQueryable Members

		public Type ElementType
		{
			get { return _inner.ElementType; }
		}

		public System.Linq.Expressions.Expression Expression
		{
			get { return _inner.Expression; }
		}

		public IQueryProvider Provider
		{
			get { return new ThrowingProvider(_inner.Provider); }
		}

		#endregion
	}

	[TestClass]
	public class QueryRetryTest
	{
		[TestMethod]
		public void QueryRetrySimple()
		{
			MediaRetryPolicy queryRetryPolicy = new TestMediaServicesClassFactory(null).GetQueryRetryPolicy();

			var mock = new ThrowingQueryable();

			var target = new MediaQueryable<string, string>(mock, queryRetryPolicy);
			Assert.AreEqual(mock.Inner.First(), target.First());
		}
	}
}
