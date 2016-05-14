//-----------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using System.Net;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Extension methods for the CloudTable class.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// An enumerator for a collection which skips table not found exceptions.
        /// </summary>
        /// <typeparam name="T">The record type.</typeparam>
        /// <param name="collection">The collection to enumerate.</param>
        /// <returns>An enumerable collection.</returns>
        public static IEnumerable<T> SkipTableNotFoundErrors<T>(this IEnumerable<T> collection)
        {
            var e = collection.GetEnumerator();
            while (true)
            {
                try
                {
                    if (!e.MoveNext())
                    {
                        yield break;
                    }
                }
                catch (StorageException se)
                {
                    if (se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    {
                        yield break;
                    }
                    else
                    {
                        throw;
                    }
                }

                yield return e.Current;
            }
        }
    }
}
