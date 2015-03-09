//-----------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Performance", "CA1824:MarkAssembliesWithNeutralResourcesLanguage", Justification = "Don't need this. We're not going to be localized for now - will revisit when we do localize.")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.BlobTransferClient.#ReadResponseStream(Microsoft.Cloud.Media.Common.Encryption.FileEncryption,System.UInt64,System.IO.FileStream,System.Byte[],System.Net.HttpWebResponse,System.Collections.Generic.KeyValuePair`2<System.Int64,System.Int32>,System.Int64&)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.BulkIngestAssetCollection.#Create(System.String[],System.String,Microsoft.Cloud.Media.SDK.Client.AssetCreationOptions,System.Boolean)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.BulkIngestAssetData.#AccountId")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.BulkIngestAssetData.#AccountId")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.BulkIngestAssetData.#Options")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.BulkIngestAssetData.#UserId")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.BulkIngestAssetData.#UserId")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.ContentKeyWriter")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", Target = "Microsoft.Cloud.Media.SDK.Client.BulkIngest.FileInfoWriter")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.TaskData.#Progress")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.LocatorData.#AccessPolicy")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Cloud.Media.SDK.Client.LocatorData.#Asset")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.BlobTransferClient.#ReadResponseStream(Microsoft.Cloud.Media.Common.Encryption.FileEncryption,System.UInt64,System.IO.FileStream,System.Byte[],System.Net.HttpWebResponse,System.Collections.Generic.KeyValuePair`2<System.Int64,System.Int32>,System.Int64&)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", Target = "Microsoft.WindowsAzure.MediaServices.Client.BulkIngest.ContentKeyWriter")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", Target = "Microsoft.WindowsAzure.MediaServices.Client.BulkIngest.FileInfoWriter")]
[assembly: SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.BulkIngest.BulkIngestAssetCollection.#Create(System.String[],System.String,Microsoft.WindowsAzure.MediaServices.Client.AssetCreationOptions,System.Boolean)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.BulkIngest.BulkIngestAssetData.#Options")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.TaskData.#Progress")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.LocatorData.#AccessPolicy")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.TaskData.#TaskInputs")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.TaskData.#TaskInputs")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.TaskData.#TaskOutputs")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.TaskData.#TaskOutputs")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.AssetPlaceholderToInstanceResolver.#EnsureSizeAndGetElement`1(System.Collections.Generic.List`1<!!0>,System.Int32,System.Func`1<!!0>)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "inputAssets", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.AssetPlaceholderToInstanceResolver.#EnsureInListsAndFindAsset(System.Collections.Generic.List`1<Microsoft.WindowsAzure.MediaServices.Client.IAsset>,System.Collections.Generic.List`1<Microsoft.WindowsAzure.MediaServices.Client.IAsset>,System.Collections.Generic.List`1<Microsoft.WindowsAzure.MediaServices.Client.IAsset>,System.String)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.AssetPlaceholderToInstanceResolver.#EnsureInListsAndFindAsset(System.Collections.Generic.List`1<Microsoft.WindowsAzure.MediaServices.Client.IAsset>,System.Collections.Generic.List`1<Microsoft.WindowsAzure.MediaServices.Client.IAsset>,System.Collections.Generic.List`1<Microsoft.WindowsAzure.MediaServices.Client.IAsset>,System.String)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.AssetPlaceholderToInstanceResolver.#ParseAssetName(System.String,Microsoft.WindowsAzure.MediaServices.Client.AssetPlaceholderToInstanceResolver+TemplateAssetType&,System.Int32&)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.AssetPlaceholderToInstanceResolver.#CreateOrGetOutputAsset(System.String)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.AssetPlaceholderToInstanceResolver.#CreateOrGetInputAsset(System.String)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.OutputAsset.#ParentAssets")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", Target = "Microsoft.WindowsAzure.MediaServices.Client.MediaProcessorData")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "readonly", Scope = "resource", Target = "Microsoft.WindowsAzure.MediaServices.Client.StringTable.resources")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.OutputAsset.#Id")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.CriticalSection.#CheckCurrentThreadHoldsLock(System.Object)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.CriticalSection.#Enter(System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.CriticalSection+DependentLockInfo.#CallStacks")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.CriticalSection+ExitOnDispose.#Finalize()")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.CriticalSection.#Enter(System.Object)")]
[assembly: SuppressMessage("Microsoft.Cryptographic.Standard", "CA5350:MD5CannotBeUsed", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.BlobTransferClient.#GetMd5HashFromStream(System.Byte[])", Justification = "MD5 used for checksum, not for encryption.")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.BlobTransferClient.#DownloadFileFromBlob(System.Uri,System.String,Microsoft.WindowsAzure.MediaServices.Client.FileEncryption,System.UInt64,Microsoft.WindowsAzure.StorageClient.CloudBlobClient,System.Threading.CancellationToken,Microsoft.WindowsAzure.StorageClient.RetryPolicy)")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "type", Target = "Microsoft.WindowsAzure.MediaServices.Client.JobData")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.CriticalSection.#CheckCurrentThreadHoldsLock(System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#ExecuteAsync`1(System.Data.Services.Client.DataServiceContext,System.Data.Services.Client.DataServiceQueryContinuation`1<!!0>,System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#ExecuteAsync`1(System.Data.Services.Client.DataServiceQuery`1<!!0>,System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#ExecuteBatchAsync(System.Data.Services.Client.DataServiceContext,System.Object,System.Data.Services.Client.DataServiceRequest[])")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#LoadPropertyAsync(System.Data.Services.Client.DataServiceContext,System.Object,System.String,System.Data.Services.Client.DataServiceQueryContinuation,System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#GetReadStreamAsync(System.Data.Services.Client.DataServiceContext,System.Object,System.Data.Services.Client.DataServiceRequestArgs,System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#LoadPropertyAsync(System.Data.Services.Client.DataServiceContext,System.Object,System.String,System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#LoadPropertyAsync(System.Data.Services.Client.DataServiceContext,System.Object,System.String,System.Uri,System.Object)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", Target = "Microsoft.WindowsAzure.MediaServices.Client.BulkIngest.AssetFileWriter")]
[assembly: SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IManifest.#BlobStorageUriForUpload")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.ManifestAssetFileCollection.#CreateAsync(Microsoft.WindowsAzure.MediaServices.Client.IManifestAsset,System.String,Microsoft.WindowsAzure.MediaServices.Client.AssetCreationOptions,System.Threading.CancellationToken)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IIngestManifest.#BlobStorageUriForUpload")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestData.#EncryptFilesAsync(System.String,System.Boolean,System.Threading.CancellationToken)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestAssetData.#Asset")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestAssetData.#Asset")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestFileCollection.#CreateAsync(Microsoft.WindowsAzure.MediaServices.Client.IIngestManifestAsset,System.String,System.Threading.CancellationToken)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.DataServiceAsyncExtensions.#ExecuteAsync(System.Data.Services.Client.DataServiceContext,System.Uri,System.Object,System.String)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.ILocator.#BaseUri")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestData.#AssetEncryptAction(System.String,System.Boolean,System.Threading.CancellationToken,System.Collections.Concurrent.ConcurrentDictionary`2<System.String,Microsoft.WindowsAzure.MediaServices.Client.IContentKey>,Microsoft.WindowsAzure.MediaServices.Client.IIngestManifestAsset)")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.AssetData.#ParentAssets")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestAssetCollection.#CreateAsync(Microsoft.WindowsAzure.MediaServices.Client.IIngestManifest,Microsoft.WindowsAzure.MediaServices.Client.IAsset,System.String[],System.Threading.CancellationToken)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestAssetCollection.#CreateAsync(Microsoft.WindowsAzure.MediaServices.Client.IIngestManifest,Microsoft.WindowsAzure.MediaServices.Client.IAsset,System.String[],System.Threading.CancellationToken)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.StorageAccountBaseCollection.#.ctor(Microsoft.WindowsAzure.MediaServices.Client.CloudMediaContext)")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Microsoft.WindowsAzure.MediaServices.Client.IngestManifestCollection.#.ctor(Microsoft.WindowsAzure.MediaServices.Client.CloudMediaContext)",Justification = "By design")]
