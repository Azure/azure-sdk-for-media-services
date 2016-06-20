//-----------------------------------------------------------------------
// <copyright file="AssetNamingSchemeResolver.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Provides resolution on asset names.
    /// </summary>
    /// <typeparam name="TInputAsset">The type of the input asset.</typeparam>
    /// <typeparam name="TOutputAsset">The type of the output asset.</typeparam>
    internal class AssetNamingSchemeResolver<TInputAsset, TOutputAsset>
        where TInputAsset : class
        where TOutputAsset : class
    {
        private readonly List<TInputAsset> _inputAssets;
        private readonly List<TOutputAsset> _tempAssets = new List<TOutputAsset>();
        private readonly List<TOutputAsset> _outputAssets = new List<TOutputAsset>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetNamingSchemeResolver&lt;TInputAsset, TOutputAsset&gt;"/> class.
        /// </summary>
        /// <param name="inputAssets">The collection of input assets.</param>
        public AssetNamingSchemeResolver(List<TInputAsset> inputAssets)
        {
            this._inputAssets = inputAssets;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetNamingSchemeResolver&lt;TInputAsset, TOutputAsset&gt;"/> class.
        /// </summary>
        public AssetNamingSchemeResolver()
            : this(new List<TInputAsset>())
        {
        }

        /// <summary>
        /// Gets the collection of input assets.
        /// </summary>
        public IList<TInputAsset> Inputs
        {
            get { return this._inputAssets; }
        }

        /// <summary>
        /// Gets the collection of output assets.
        /// </summary>
        public IList<TOutputAsset> Outputs
        {
            get { return this._outputAssets; }
        }

        /// <summary>
        /// Gets the collection of temporary assets.
        /// </summary>
        public IList<TOutputAsset> Temporaries
        {
            get { return this._tempAssets; }
        }

        /// <summary>
        /// Gets the asset ID for the specified object.
        /// </summary>
        /// <param name="obj">The object that represents an input or output asset.</param>
        /// <returns>The asset ID.</returns>
        public string GetAssetId(object obj)
        {
            TInputAsset asset = obj as TInputAsset;
            if (asset != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "JobInputAsset({0})", CalcIndex(asset, this._inputAssets));
            }

            OutputAsset outputAsset = obj as OutputAsset;
            if (outputAsset != null)
            {
                TOutputAsset toutputAsset = outputAsset as TOutputAsset;
                return string.Format(CultureInfo.InvariantCulture, "JobOutputAsset({0})", CalcIndex(toutputAsset, this._outputAssets));
            }

            throw new InvalidCastException(StringTable.ErrorInvalidTaskInput);
        }

        public bool IsExistingOutputAsset(object obj)
        {
            OutputAsset outputAsset = obj as OutputAsset;
            if (outputAsset != null)
            {
                TOutputAsset toutputAsset = outputAsset as TOutputAsset;
                return Outputs.Contains(toutputAsset);
            }

            return false;
        }


        private static int CalcIndex<T>(T obj, List<T> list)
        {
            int index = list.IndexOf(obj);
            if (index == -1)
            {
                list.Add(obj);
                index = list.Count - 1;
            }

            return index;
        }
    }
}
