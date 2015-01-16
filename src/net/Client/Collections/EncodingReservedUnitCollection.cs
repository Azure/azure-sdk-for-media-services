//-----------------------------------------------------------------------
// <copyright file="EncodingReservedUnitCollection.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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


namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public class EncodingReservedUnitCollection : CloudBaseCollection<IEncodingReservedUnit>
    {
        /// <summary>
        /// The name of the storage account processor set.
        /// </summary>
        public const string EncodingReservedUnitSet = "EncodingReservedUnitTypes";
        /// <summary>
        /// Initializes a new instance of the <see cref="EncodingReservedUnitCollection"/> class
        /// </summary>
        public EncodingReservedUnitCollection()
            : base(null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EncodingReservedUnitCollection"/> class.
        /// </summary>
        /// <param name="mediaContext">The media context.</param>
        internal EncodingReservedUnitCollection(MediaContextBase mediaContext)
            : base(mediaContext)
        {
            MediaContext = mediaContext;
            MediaServicesClassFactory factory = MediaContext.MediaServicesClassFactory;
            this.Queryable = factory.CreateDataServiceContext().CreateQuery<IEncodingReservedUnit, EncodingReservedUnitData>(EncodingReservedUnitSet);
        }

    }
}
