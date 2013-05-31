//-----------------------------------------------------------------------
// <copyright file="AssetPlaceholderToInstanceResolver.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.MediaServices.Client
{

    /// <summary>
    /// Provides resolution of an asset placeholder to an asset instance.
    /// </summary>
    internal class AssetPlaceholderToInstanceResolver
    {
        private static readonly Regex _jobInputExpression = new Regex(@"^JobInputAsset\((\d+)\)$", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex _jobOutputExpression = new Regex(@"^JobOutputAsset\((\d+)\)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly List<IAsset> _outputAssets = new List<IAsset>();

        /// <summary>
        /// Describes the different template types.
        /// </summary>
        internal enum TemplateAssetType
        {
            /// <summary>
            /// Input to a job template.
            /// </summary>
            JobTemplateInput,

            /// <summary>
            /// Job ouput template.
            /// </summary>
            JobOutput
        }

        /// <summary>
        /// Creates or gets the input asset.
        /// </summary>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns>The created or retrieved asset.</returns>
        public IAsset CreateOrGetInputAsset(string assetName)
        {
            IAsset inputAsset = EnsureInListsAndFindAsset(this._outputAssets, assetName) as IAsset;

            return inputAsset;
        }

        /// <summary>
        /// Creates or gets the output asset.
        /// </summary>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns>The created or retrieved asset.</returns>
        public IAsset CreateOrGetOutputAsset(string assetName)
        {
            IAsset outputAsset = EnsureInListsAndFindAsset(this._outputAssets, assetName) as IAsset;

            if (outputAsset == null)
            {
                throw new InvalidDataException(StringTable.ErrorCannotParseOutout);
            }

            return outputAsset;
        }
       
        private static void ParseAssetName(string assetName, out TemplateAssetType assetType, out int assetIndex)
        {
            Match match = _jobInputExpression.Match(assetName);

            if (match.Success)
            {
                assetType = TemplateAssetType.JobTemplateInput;
            }
            else
            {
                match = _jobOutputExpression.Match(assetName);

                if (match.Success)
                {
                    assetType = TemplateAssetType.JobOutput;
                }
                else
                {
                    throw new InvalidDataException(StringTable.ErrorTaskBodyMalformed);
                }
            }

            assetIndex = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        private static T EnsureSizeAndGetElement<T>(IList<T> list, int size, Func<T> creator) where T : class
        {
            if (list.Count < size)
            {
                for (int i = list.Count; i < size; i++)
                {
                    list.Add(null);
                }
            }

            T returnValue = list[size - 1];
            if (returnValue == null)
            {
                returnValue = list[size - 1] = creator();
            }

            return returnValue;
        }

        private static object EnsureInListsAndFindAsset(IList<IAsset> outputAssets, string assetName)
        {
            TemplateAssetType assetType;
            int assetIndex;
            ParseAssetName(assetName, out assetType, out assetIndex);

            if (assetType == TemplateAssetType.JobOutput)
            {
                return EnsureSizeAndGetElement(outputAssets, assetIndex + 1, () => new OutputAsset() { Name = assetName });
            }

            return null;
        }
    }
}
