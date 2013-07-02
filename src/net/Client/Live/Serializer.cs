// Copyright 2012 Microsoft Corporation
// 
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

using System.Web.Script.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Serializes and deserializes objects to/from string formatted for consumption by REST API.
    /// </summary>
    internal static class Serializer
    {
        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <typeparam name="T">Object type to produce.</typeparam>
        /// <param name="s">Serialized object.</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return default(T);
            }

            return new JavaScriptSerializer().Deserialize<T>(s);

        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Serialized object.</returns>
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
            {
                return null;
            }
            
            return new JavaScriptSerializer().Serialize(obj);
        }
    }
}
