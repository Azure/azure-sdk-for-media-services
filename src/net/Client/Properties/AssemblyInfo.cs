﻿//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Microsoft.WindowsAzure.MediaServices.Client.dll")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Windows Azure Media Services")]
[assembly: AssemblyCopyright("Copyright © 2012 Microsoft Corp.")]
[assembly: AssemblyTrademark("Microsoft ® is a registered trademark of Microsoft Corporation.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d6f3a510-3185-455a-98c3-2333054322c7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("3.0.0.3")]
[assembly: AssemblyFileVersion("3.0.0.3")]
[assembly: NeutralResourcesLanguage("en-US")]

//For delay signing specify PublicKey for each friendly assembly
//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.WindowsAzure.MediaServices.Client.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.WindowsAzure.MediaServices.Client.Tests.Scenario")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.WindowsAzure.MediaServices.Client.Tests.Common")]