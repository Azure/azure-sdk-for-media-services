//-----------------------------------------------------------------------
// <copyright file="KnownApiVersions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters
{


    /// <summary>
    /// The list of API versions.
    /// </summary>
    public static class KnownApiVersions
    {
        /// <summary>
        /// Version 2.0.
        /// </summary>
        public static readonly Version Version2 = new Version(major: 2, minor: 0);
        public static readonly Version Version2_1 = new Version(major: 2, minor: 1);
        public static readonly Version Version2_2 = new Version(major: 2, minor: 2);
        public static readonly Version Version2_3 = new Version(major: 2, minor: 3);
        public static readonly Version Version2_4 = new Version(major: 2, minor: 4);
        public static readonly Version Version2_5 = new Version(major: 2, minor: 5);
        public static readonly Version Version2_6 = new Version(major: 2, minor: 6);
        public static readonly Version Version2_7 = new Version(major: 2, minor: 7);
        public static readonly Version Version2_8 = new Version(major: 2, minor: 8);
        public static readonly Version Version2_9 = new Version(major: 2, minor: 9);
        public static readonly Version Version2_10 = new Version(major: 2, minor: 10);
        public static readonly Version Version2_11 = new Version(major: 2, minor: 11);
        public static readonly Version Version2_12 = new Version(major: 2, minor: 12);
        public static readonly Version Version2_13 = new Version(major: 2, minor: 13);
        public static readonly Version Version2_14 = new Version(major: 2, minor: 14);
        public static readonly Version Version2_15 = new Version(major: 2, minor: 15);
        public static readonly Version Version2_16 = new Version(major: 2, minor: 16);
        public static readonly Version Version2_17 = new Version(major: 2, minor: 17);

        /// <summary>
        /// Gets the Media Service API version.
        /// </summary>
        public static Version Current
        {
            get { return Version2_17; }
        }
    }
}
