using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
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

	class TrowingQueryable : IQueryable<string>
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

			var mock = new TrowingQueryable();

			var target = new MediaQueryable<string, string>(mock, queryRetryPolicy);
			Assert.AreEqual(mock.Inner.First(), target.First());
		}
	}
}
