//-----------------------------------------------------------------------
// <copyright file="JobTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Configuration;
using System.Data.Services.Client;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public partial class JobTests
    {

        public const string ExpressionEncoder = "Windows Azure Media Encoder";
        public const string WameV1Preset = "H.264 256k DSL CBR";
        public const string WameV2Preset = "H264 Broadband SD 4x3";
        public const string Mp4ToSmoothStreamsTask = "MP4 to Smooth Streams Task";
        public const string PlayReadyProtectionTask = "PlayReady Protection Task";
        public const string SmoothToHlsTask = "Smooth Streams to HLS Task";
        public const string StrorageDecryptionProcessor = "Storage Decryption";
        public const string MediaEncryptor = "Windows Azure Media Encryptor";
        public const string MediaPackager = "Windows Azure Media Packager";

        private CloudMediaContext _mediaContext;
        private string _smallWmv;
        private const int JobTimeOutInMinutes = 25;
        private const string NamePrefix = "JobTests_";
        private const int InitialJobPriority = 1;


        /// <summary>
        ///     Gets or sets the test context which provides information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            _smallWmv = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv);
        }

        // media processor versions
        public static string GetWamePreset(IMediaProcessor mediaProcessor)
        {
            var mpVersion = new Version(mediaProcessor.Version);
            if (mpVersion.Major == 1)
            {
                return WameV1Preset;
            }
            else
            {
                return WameV2Preset;
            }
        }


        public static void VerifyAllTasksFinished(string jobId)
        {
            CloudMediaContext context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IJob job2 = context2.Jobs.Where(c => c.Id == jobId).Single();
            Assert.AreEqual(JobState.Finished, job2.State);

            foreach (ITask task in job2.Tasks)
            {
                Assert.AreEqual(JobState.Finished, task.State);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldCreateJobPreset()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("Job 1");
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, name, mediaProcessor, GetWamePreset(mediaProcessor), asset, TaskOptions.None);
            Task task = job.GetExecutionProgressTask(CancellationToken.None);
            task.Wait();
            Assert.AreEqual(JobState.Finished, job.State);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void CreateJobFromTemplate()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("Job For Template");

            IJob job = _mediaContext.Jobs.Create(name);
            job.Priority = InitialJobPriority;
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, GetWamePreset(mediaProcessor), TaskOptions.None);
            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("JobTemplateOutPutAsset", AssetCreationOptions.None);

            DateTime timebeforeSubmit = DateTime.UtcNow;
            job.Submit();
            Task jobRunningTask = job.GetExecutionProgressTask(CancellationToken.None);
            jobRunningTask.Wait();
            IJobTemplate template = job.SaveAsTemplate("JobTemplate" + Guid.NewGuid().ToString().Substring(0, 10));
            var jobfromTemplate = _mediaContext.Jobs.Create("JobFromTemplate" + Guid.NewGuid().ToString().Substring(0, 10), template, new[]{asset});
            jobfromTemplate.Submit();
            jobRunningTask = jobfromTemplate.GetExecutionProgressTask(CancellationToken.None);
            jobRunningTask.Wait();

            var refreshed = _mediaContext.Jobs.Where(c => c.Id == jobfromTemplate.Id).FirstOrDefault();
            Assert.IsNotNull(refreshed);


        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldReportJobProgress()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("Job 1");
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, name, mediaProcessor, GetWamePreset(mediaProcessor), asset, TaskOptions.None);
            bool progressChanged = false;
            var task = Task.Factory.StartNew(delegate
             {
                 while (!IsFinalJobState(job.State))
                 {
                     Thread.Sleep(100);
                     double progress = job.Tasks[0].Progress;
                     if (progress > 0 && progress < 100)
                     {
                         progressChanged = true;
                     }
                     job.Refresh();
                 }
             });
            task.Wait();
            task.ThrowIfFaulted();
            Assert.AreEqual(JobState.Finished, job.State);
            Assert.IsTrue(progressChanged, "Task progress has not been changed while job is expected to be finished");

        }



        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldQueryJobByStartTime()
        {
            DateTime startDateTime = DateTime.UtcNow;
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("Job 1");
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, name, mediaProcessor, GetWamePreset(mediaProcessor), asset, TaskOptions.None);
            Task task = job.GetExecutionProgressTask(CancellationToken.None);
            task.Wait();
            List<IJob> jobs = _mediaContext.Jobs.Where(j => j.StartTime > startDateTime && j.StartTime < DateTime.UtcNow).ToList();

            Assert.IsTrue(jobs.Count > 0);
        }

        private static bool IsFinalJobState(JobState jobState)
        {
            return (jobState == JobState.Canceled) || (jobState == JobState.Error) || (jobState == JobState.Finished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldGenerateMetadataFile()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.None);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("ShouldSplitMetadataLost");

            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, name, mediaProcessor, "H264 Smooth Streaming 720p", asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);

            IJob refreshedJob = _mediaContext.Jobs.Where(c => c.Id == job.Id).Single();
            bool ok = refreshedJob.Tasks.Single().OutputAssets.Single().AssetFiles.AsEnumerable().Select(f => f.Name).Contains("SmallWmv_manifest.xml");
            Assert.IsTrue(ok);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldFinishJobWithThumbnailPreset()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string presetXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <Thumbnail Size=""80,60"" Type=""Jpeg"" Filename=""{OriginalFilename}_{ThumbnailTime}.{DefaultExtension}"">
                  <Time Value=""0:0:0""/>
                  <Time Value=""0:0:3"" Step=""0:0:0.25"" Stop=""0:0:10""/>
                </Thumbnail>";
            string name = GenerateName("ShouldFinishJobWithSuccessWhenPresetISUTF8");
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, name, mediaProcessor, presetXml, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
            var task = job.Tasks.First();
            var assets = task.OutputAssets.ToList();
            var firstasset = assets.First();
            var files = firstasset.AssetFiles.ToList();
            Assert.IsNotNull(files);
            Assert.IsTrue(files.Count >1);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldContainTaskHistoryEventsOnceJobFinished()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("ShouldContainTaskHistoryEventsOnceJobFinished");
            string preset = GetWamePreset(mediaProcessor);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, name, mediaProcessor, preset, asset, TaskOptions.None);
            ITask task = job.Tasks.FirstOrDefault();
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.HistoricalEvents, "HistoricalEvents should not be null for submitted job");
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
            Assert.IsTrue(task.HistoricalEvents.Count > 0, "HistoricalEvents should not be empty after job has been finished");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(ArgumentException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldThrowTryingToCreateJobWithOneTaskAndNoOutput()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IJob job = _mediaContext.Jobs.Create("CreateJobWithOneTaskAndNoOutput");
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            ITask task = job.Tasks.AddNew("Task1", processor, GetWamePreset(processor), TaskOptions.None);
            task.InputAssets.Add(asset);
            job.Submit();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowTryingTocreateCreateJobWithNoTasks()
        {
            try
            {
                IJob job = _mediaContext.Jobs.Create("CreateJobWithNoTasks");
                job.Submit();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("There must be at least one task.", ex.Message, "Wrong exception message");
                throw ex;
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSubmitAndFinishJobWithOneTaskEmptyConfiguration()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IJob job = _mediaContext.Jobs.Create("CreateJobWithOneTaskEmptyConfiguration");
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpStorageDecryptorName);
            ITask task = job.Tasks.AddNew("Task1", processor, String.Empty, TaskOptions.None);
            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("Output", AssetCreationOptions.None);
            job.Submit();
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldFinishJobWithErrorWithInvalidPreset()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldFinishJobWithErrorWithInvalidPreset"), processor, "Some wrong Preset", asset, TaskOptions.None);
            Action<string> verify = id =>
            {
                IJob job2 = _mediaContext.Jobs.Where(c => c.Id == id).SingleOrDefault();
                Assert.IsNotNull(job2);
                Assert.IsNotNull(job2.Tasks);
                Assert.AreEqual(1, job2.Tasks.Count);
                Assert.IsNotNull(job2.Tasks[0].ErrorDetails);
                Assert.AreEqual(1, job2.Tasks[0].ErrorDetails.Count);
                Assert.IsNotNull(job2.Tasks[0].ErrorDetails[0]);
                Assert.AreEqual("ErrorParsingConfiguration", job2.Tasks[0].ErrorDetails[0].Code);
            };
            WaitForJob(job.Id, JobState.Error, verify);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCancelJobAfterSubmission()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldCancelJobAfterSubmission"), processor, GetWamePreset(processor), asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Processing, (string id) => { });
            job.Cancel();
            WaitForJob(job.Id, JobState.Canceling, (string id) => { });
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Thumbnail.xml", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldFinishJobCreatedFromThumbnailXml()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            string xmlPreset = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailXml);
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldFinishJobCreatedFromThumbnailXml"), processor, xmlPreset, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\ThumbnailWithZeroStep.xml", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldFinishJobWithZeroStepThumbnail()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            string xmlPreset = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailWithZeroStepXml);
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldFinishJobWithZeroStepThumbnail"), processor, xmlPreset, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Media\SmallMp41.mp4", "Media")]
        [DeploymentItem(@"Configuration\multi.xml", "Configuration")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateJobWithMultipleAssetsAndValidateParentLinks()
        {
            // Create multiple assets, set them as parents for a job, and validate that the parent links are set.
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset asset2 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            IAsset asset3 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.StorageEncrypted);

            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.MultiConfig);

            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, configuration, TaskOptions.None);

            task.InputAssets.Add(asset1);
            task.InputAssets.Add(asset2);
            task.InputAssets.Add(asset3);
            task.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None);
            job.Submit();

            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished, delegate(double d) { Console.WriteLine(d); });

            Assert.IsTrue(job.OutputMediaAssets[0].ParentAssets.Count == 3);
            IEnumerable<string> parentIds = job.OutputMediaAssets[0].ParentAssets.Select(a => a.Id);
            Assert.IsTrue(parentIds.Contains(asset1.Id));
            Assert.IsTrue(parentIds.Contains(asset2.Id));
            Assert.IsTrue(parentIds.Contains(asset3.Id));
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Media\SmallMp41.mp4", "Media")]
        [DeploymentItem(@"Configuration\multi.xml", "Configuration")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSubmitAndFinishJobWithMultipleAssetAndVerifyOrderOfInputAssets()
        {
            // Create multiple assets, set them as parents for a job, and validate that the parent links are set.
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            IAsset asset2 = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset asset3 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset2.Name = "SmallWmv";
            asset3.Name = "SmallMP41";
            asset1.Update();
            asset2.Update();
            asset3.Update();

            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.MultiConfig);

            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, configuration, TaskOptions.None);

            task.InputAssets.Add(asset1);
            task.InputAssets.Add(asset2);
            task.InputAssets.Add(asset3);
            task.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None);
            job.Submit();

            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);

            Assert.IsTrue(job.InputMediaAssets.Count == 3);
            Assert.IsTrue(job.InputMediaAssets[0].Name == "SmallWmv2");
            Assert.IsTrue(job.InputMediaAssets[1].Name == "SmallWmv");
            Assert.IsTrue(job.InputMediaAssets[2].Name == "SmallMP41");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Media\SmallMp41.mp4", "Media")]
        [DeploymentItem(@"Media\Thumbnail.xml", "Media")]
        public void ShouldSubmitAndFinishJobWithMultipleTasksAndSharedOutputAsset()
        {
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset1.Update();


            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailXml);

            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            
            ITask task1 = job.Tasks.AddNew("Task1", mediaProcessor, configuration, TaskOptions.None);
            task1.InputAssets.Add(asset1);
            IAsset output = task1.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);
            
            ITask task2 = job.Tasks.AddNew("Task2", mediaProcessor, configuration, TaskOptions.None);
            task2.InputAssets.Add(asset1);
            task2.OutputAssets.Add(output);
           
            job.Submit();

            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
            Assert.IsTrue(job.OutputMediaAssets.Count == 1);
            Assert.AreEqual(job.Tasks[0].OutputAssets[0].Id, job.Tasks[1].OutputAssets[0].Id, "Output assets are not the same");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Configuration\Thumbnail.txt", "Configuration")]
        [DeploymentItem(@"Configuration\Proxy.txt", "Configuration")]
        [DeploymentItem(@"Configuration\MBR.txt", "Configuration")]
        public void ShouldSubmitAndFinishJobWithMesAndMultipleTasksAndSharedOutputAsset()
        {
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset1.Update();

            string configuration1 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailConfig);
            string configuration2 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ProxyConfig);
            string configuration3 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.MbrConfig);


            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MesName);

            ITask task1 = job.Tasks.AddNew("Task1", mediaProcessor, configuration1, TaskOptions.DoNotCancelOnJobFailure | TaskOptions.DoNotDeleteOutputAssetOnFailure);
            task1.InputAssets.Add(asset1);
            task1.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);

            ITask task2 = job.Tasks.AddNew("Task2", mediaProcessor, configuration2, TaskOptions.DoNotCancelOnJobFailure | TaskOptions.DoNotDeleteOutputAssetOnFailure);
            task2.InputAssets.Add(asset1);
            IAsset outputAsset = task2.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.AdaptiveStreaming);


            ITask task3 = job.Tasks.AddNew("Task3", mediaProcessor, configuration3, TaskOptions.DoNotCancelOnJobFailure | TaskOptions.DoNotDeleteOutputAssetOnFailure);
            task3.InputAssets.Add(asset1);
            task3.OutputAssets.Add(outputAsset);

            job.Submit();

            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);

            Assert.IsTrue(job.OutputMediaAssets.Count == 2);
            Assert.AreEqual(job.Tasks[1].OutputAssets[0].Id, job.Tasks[2].OutputAssets[0].Id, "Output assets are not the same");
            //Assert.AreEqual(AssetType.SmoothStreaming, job.Tasks[1].OutputAssets[0].AssetType);

            string workingDir = Path.GetTempPath();
            IAssetFile ismAsset =
                job.Tasks[1].OutputAssets[0].AssetFiles.Where(f => f.IsPrimary).SingleOrDefault();
            Assert.IsNotNull(ismAsset);
            string fullPath = Path.Combine(Path.GetTempPath(), ismAsset.Name);
            ismAsset.Download(fullPath);

            var xDocument = XDocument.Load(fullPath);
            XNamespace ns = xDocument.Root.Name.Namespace;
            var xElement =
                xDocument.Descendants(ns + "meta").SingleOrDefault(f => f.Attribute("name").Value == "formats");
            Assert.IsNotNull(xElement, "Missing format element in primary file");
            Assert.AreEqual(xElement.Attribute("content").Value, "vod-fmp4", "Not valid adaptive streaming");
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Configuration\Proxy.txt", "Configuration")]
        public void TestJobWithTaskNotificationToBothAzureQueueAndWebHookEndPoint()
        {
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset1.Update();

            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ProxyConfig);
            string webhookEndpoint = ConfigurationManager.AppSettings["WebHookEndPointWithoutEncryption"];

            INotificationEndPoint endpoint1 = _mediaContext.NotificationEndPoints.Create("endpoint1", 
                NotificationEndPointType.AzureQueue,
                "tasknotificationqueue");
            INotificationEndPoint endpoint2 = _mediaContext.NotificationEndPoints.Create("endpoint2",
                 NotificationEndPointType.WebHook, webhookEndpoint);

            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MesName);

            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, configuration, TaskOptions.None);
            task.InputAssets.Add(asset1);
            task.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);
            task.TaskNotificationSubscriptions.AddNew(NotificationJobState.All, endpoint1, true);
            task.TaskNotificationSubscriptions.AddNew(NotificationJobState.All, endpoint2, true);

            job.Submit();
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Configuration\Proxy.txt", "Configuration")]
        public void TestJobWithTaskNotificationToWebHookEndPointWithEncryption()
        {
            byte[] bytes = new byte[64];
           
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.None);
            asset1.Name = "SmallWmv2";
            asset1.Update();

            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ProxyConfig);
            string webhookEndpoint = ConfigurationManager.AppSettings["WebHookEndPointWithEncryption"];

            INotificationEndPoint endpoint = _mediaContext.NotificationEndPoints.Create("endpoint2",
                 NotificationEndPointType.WebHook, webhookEndpoint, bytes);
            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MesName);

            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, configuration, TaskOptions.None);
            task.InputAssets.Add(asset1);
            task.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);
            task.TaskNotificationSubscriptions.AddNew(NotificationJobState.All, endpoint, true);

            job.Submit();
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }
        
        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Thumbnail.txt", "Configuration")]
        [DeploymentItem(@"Configuration\Proxy.txt", "Configuration")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        public void ShouldFailJobWhenTryingAddOutputAssetFromDifferentJobToTask()
        {
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset1.Update();

            string configuration1 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailConfig);
            string configuration2 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ProxyConfig);

            IJob job1 = _mediaContext.Jobs.Create("Test1");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MesName);

            ITask task1 = job1.Tasks.AddNew("Task1", mediaProcessor, configuration1, TaskOptions.None);
            task1.InputAssets.Add(asset1);
            IAsset outputAsset = task1.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);

            IJob job2 = _mediaContext.Jobs.Create("Test2");
            ITask task2 = job2.Tasks.AddNew("Task2", mediaProcessor, configuration2, TaskOptions.None);
            task2.InputAssets.Add(asset1);

            try
            {
                task2.OutputAssets.Add(outputAsset);
                Assert.Fail();
            }
            catch(ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains(StringTable.ErrorAddAssetToOutputAssetsOfTask));
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Thumbnail.txt", "Configuration")]
        [DeploymentItem(@"Configuration\Proxy.txt", "Configuration")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        public void ShouldSubmitJobWhenCreatingTaskWithNoCancelNoDeleteOption()
        {
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset1.Update();

            string configuration1 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailConfig);
            string configuration2 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ProxyConfig);

            IJob job1 = _mediaContext.Jobs.Create("Test1");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MesName);

            ITask task1 = job1.Tasks.AddNew("Task1", mediaProcessor, configuration1, TaskOptions.DoNotDeleteOutputAssetOnFailure);
            task1.InputAssets.Add(asset1);
            IAsset outputAsset = task1.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);

            ITask task2 = job1.Tasks.AddNew("Task2", mediaProcessor, configuration2, TaskOptions.DoNotCancelOnJobFailure);
            task2.InputAssets.Add(asset1);
            task2.OutputAssets.Add(outputAsset);

            job1.Submit();

            WaitForJob(job1.Id, JobState.Finished, VerifyAllTasksFinished);
            Assert.IsTrue(job1.OutputMediaAssets.Count == 1);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Thumbnail.txt", "Configuration")]
        [DeploymentItem(@"Configuration\Proxy.txt", "Configuration")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        public void ShouldFailJobWhenTryingAddInputAssetToOutputAssetOfTask()
        {
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset1.Update();

            string configuration1 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailConfig);
            string configuration2 = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ProxyConfig);

            IJob job1 = _mediaContext.Jobs.Create("Test1");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MesName);

            ITask task1 = job1.Tasks.AddNew("Task1", mediaProcessor, configuration1, TaskOptions.None);
            task1.InputAssets.Add(asset1);
            IAsset outputAsset = task1.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);

            IJob job2 = _mediaContext.Jobs.Create("Test2");
            ITask task2 = job2.Tasks.AddNew("Task2", mediaProcessor, configuration2, TaskOptions.None);
            task2.InputAssets.Add(asset1);

            try
            {
                task2.OutputAssets.Add(asset1);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(ex.Message.Contains(StringTable.ErrorAddingNonOutputAssetToTask));
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        public void ShouldSubmitAndFinishJobWhenTryingAddOutputAssetFromSameTask()
        {
            IAsset asset1 = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            asset1.Name = "SmallWmv2";
            asset1.Update();

            const string configuration1 = @"SaaS Thumbnail";

            IJob job1 = _mediaContext.Jobs.Create("Test1");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MesName);

            ITask task1 = job1.Tasks.AddNew("Task1", mediaProcessor, configuration1, TaskOptions.None);
            task1.InputAssets.Add(asset1);
            IAsset outputAsset = task1.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None, formatOption: AssetFormatOption.None);

            task1.OutputAssets.Add(outputAsset);
            job1.Submit();

            WaitForJob(job1.Id, JobState.Finished, VerifyAllTasksFinished);
            Assert.IsTrue(job1.OutputMediaAssets.Count == 1);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Thumbnail.xml", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [Priority(0)]
        [TestCategory("Bvt")]
        public void ShouldSubmitAndFinishChainedTasks()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);

            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, GetWamePreset(mediaProcessor), TaskOptions.None);
            task.InputAssets.Add(asset);
            IAsset asset2 = task.OutputAssets.AddNew("Another asset");

            string xmlPreset = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailXml);
            ITask task2 = job.Tasks.AddNew("Task2", mediaProcessor, xmlPreset, TaskOptions.None);
            task2.InputAssets.Add(asset2);
            task2.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None);
            job.Submit();


            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Thumbnail.xml", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [Priority(1)]
        public void ShouldSubmitAndFinishChainedTasksUsingParentOverload()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);

            IJob job = _mediaContext.Jobs.Create("Test");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, GetWamePreset(mediaProcessor), TaskOptions.None);
            task.InputAssets.Add(asset);
            IAsset asset1 = task.OutputAssets.AddNew("output asset");

            string xmlPreset = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.ThumbnailXml);
            ITask task2 = job.Tasks.AddNew("Task2", mediaProcessor, xmlPreset, TaskOptions.None, task);
            task2.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.None);
            job.Submit();
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }



        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\MP4 to Smooth Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\SmallMP41.mp4", "Media")]
        public void ShouldSubmitAndFihishMp4ToSmoothJob()
        {
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.DefaultMp4ToSmoothConfig);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpPackagerName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFihishMp4ToSmoothJob"), mediaProcessor, configuration, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);

        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\AudioEncodingPreset.xml", "Configuration")]
        [DeploymentItem(@"Media\SmallMP41.mp4", "Media")]
        public void ShouldSubmitAndFihishMp4ToAudioJob()
        {
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.AudioOnlyConfig);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.None);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFihishMp4ToAudioJob"), mediaProcessor, configuration, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\PlayReady Protection.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        [Priority(0)]
        public void ShouldSubmitAndFinishPlayReadyProtectionJobWithSeed()
        {
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.PlayReadyConfig);

            IAsset asset = CreateSmoothAsset();
            IMediaProcessor mediaEncryptor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncryptorName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFinishPlayReadyProtectionJob"), mediaEncryptor, configuration, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        internal static string UpdatePlayReadyConfigurationXML(Guid keyId, byte[] keyValue, Uri licenseAcquisitionUrl, string originalXmlConfiguration)
        {
            XNamespace xmlns = "http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#";

            StringReader stringReader = new StringReader(originalXmlConfiguration);
            XmlReader reader = XmlReader.Create(stringReader, null);
            XDocument doc = XDocument.Load(reader);

            var licenseAcquisitionUrlEl = doc
                    .Descendants(xmlns + "property")
                    .Where(p => p.Attribute("name").Value == "licenseAcquisitionUrl")
                    .FirstOrDefault();
            var contentKeyEl = doc
                    .Descendants(xmlns + "property")
                    .Where(p => p.Attribute("name").Value == "contentKey")
                    .FirstOrDefault();
            var keyIdEl = doc
                    .Descendants(xmlns + "property")
                    .Where(p => p.Attribute("name").Value == "keyId")
                    .FirstOrDefault();

            // Update the "value" property.
            if (licenseAcquisitionUrlEl != null)
            {
                licenseAcquisitionUrlEl.Attribute("value").SetValue(licenseAcquisitionUrl.ToString());
            }

            if (contentKeyEl != null)
            {
                contentKeyEl.Attribute("value").SetValue(Convert.ToBase64String(keyValue));
            }

            if (keyIdEl != null)
            {
                keyIdEl.Attribute("value").SetValue(keyId);
            }

            return doc.ToString();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\PlayReady Protection_ContentKey.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        [Priority(0)]
        public void ShouldSubmitAndFinishPlayReadyProtectionJobWithKey()
        {
            // Use the published PlayReady Test Key Seed to generate a content key based on a randomly generated Guid.
            // We could also use a cryptographically sound random number generator here like RNGCryptoServiceProvider.
            byte[] keySeed = Convert.FromBase64String("XVBovsmzhP9gRIZxWfFta3VVRPzVEWmJsazEJ46I");
            Guid keyId = Guid.NewGuid();
            byte[] keyValue = CommonEncryption.GeneratePlayReadyContentKey(keySeed, keyId);
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.PlayReadyConfigWithContentKey);
            configuration = UpdatePlayReadyConfigurationXML(keyId, keyValue, new Uri("http://www.fakeurl.com"), configuration);

            IAsset asset = CreateSmoothAsset();
            IMediaProcessor mediaEncryptor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncryptorName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFinishPlayReadyProtectionJob"), mediaEncryptor, configuration, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [Priority(0)]
        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Smooth Streams to Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ShouldSubmitAndFinishSmoothToHlsJob()
        {
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.SmoothToHlsConfig);

            IAsset asset = CreateSmoothAsset();
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpPackagerName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFinishSmoothToHlsJob"), mediaProcessor, configuration, asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [Priority(1)]
        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Smooth Streams to Encrypted Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ShouldSubmitAndFinishSmoothToHlsEncryptedJob()
        {
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.SmoothToEncryptHlsConfig);

            IAsset asset = CreateSmoothAsset();

            IMediaProcessor mediaPackager = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpPackagerName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFinishSmoothToHlsEncryptedJob"), mediaPackager, configuration, asset, TaskOptions.ProtectedConfiguration);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Smooth Streams to Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Configuration\PlayReady Protection_ContentKey.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        [Priority(0)]
        public void ShouldSubmitAndFinishPlayReadyProtectedHlsJob()
        {
            IAsset asset = CreateSmoothAsset();

            // Use the published PlayReady Test Key Seed to generate a content key based on a randomly generated Guid.
            // We could also use a cryptographically sound random number generator here like RNGCryptoServiceProvider.
            byte[] keySeed = Convert.FromBase64String("XVBovsmzhP9gRIZxWfFta3VVRPzVEWmJsazEJ46I");
            Guid keyId = Guid.NewGuid();
            byte[] keyValue = CommonEncryption.GeneratePlayReadyContentKey(keySeed, keyId);
            string encryptionConfiguration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.PlayReadyConfigWithContentKey);
            encryptionConfiguration = UpdatePlayReadyConfigurationXML(keyId, keyValue, new Uri("http://www.fakeurl.com"), encryptionConfiguration);

            IJob job = _mediaContext.Jobs.Create("PlayReady Protected Hls Job");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncryptorName);
            ITask task = job.Tasks.AddNew("PlayReady Encryption Task", mediaProcessor, encryptionConfiguration, TaskOptions.ProtectedConfiguration);
            task.InputAssets.Add(asset);
            IAsset asset2 = task.OutputAssets.AddNew("PlayReady Protected Smooth");

            string smoothToHlsConfiguration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.SmoothToHlsConfig);
            ITask task2 = job.Tasks.AddNew("Smooth to Hls conversion task", mediaProcessor, smoothToHlsConfiguration, TaskOptions.None);
            task2.InputAssets.Add(asset2);
            task2.OutputAssets.AddNew("JobOutput", options: AssetCreationOptions.CommonEncryptionProtected);
            job.Submit();

            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\MP4 to Smooth Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\SmallMP41.mp4", "Media")]
        [TestCategory("Bvt")]
        public void ShouldSubmitAndFinishMp4ToSmoothJobWithStorageProtectedInputsAndOutputs()
        {
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.DefaultMp4ToSmoothConfig);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.StorageEncrypted);
            IJob job = _mediaContext.Jobs.Create("MP4 to Smooth with protected input and output assets");
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpPackagerName);
            ITask task = job.Tasks.AddNew(MediaPackager, mediaProcessor, configuration, TaskOptions.None);
            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("Output encrypted", AssetCreationOptions.StorageEncrypted);
            job.Submit();
            Assert.IsNotNull(task.InputAssets);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\PlayReady Protection.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ShouldSubmitAndFinishPlayReadyProtectionJobWithStorageAndConfigurationEncryption()
        {
            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.PlayReadyConfig);

            IAsset asset = CreateSmoothAsset();
            IMediaProcessor mediaEncryptor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncryptorName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFinishPlayReadyProtectionJobWithStorageAndConfigurationEncryption"), mediaEncryptor, configuration, asset, TaskOptions.ProtectedConfiguration);
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\EncodePlusEncryptWithEE.xml", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSubmitAndFinishEETaskWithStorageProtectedInputAndClearOutput()
        {
            //
            //  This test uses the same preset as the EE DRM tests but does not apply
            //  common encryption.  This preset gets split into multiple subtasks by EE.
            //

            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);

            // Load the EE preset to create a smooth streaming presentation with PlayReady protection
            string xmlPreset = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.EncodePlusEncryptWithEeXml);

            // Remove the DRM Section to produce clear content
            var doc = new XmlDocument();
            doc.LoadXml(xmlPreset);

            XmlNodeList drmNodes = doc.GetElementsByTagName("Drm");
            Assert.AreEqual(1, drmNodes.Count);

            XmlNode drmNode = drmNodes[0];
            drmNode.ParentNode.RemoveChild(drmNode);

            xmlPreset = doc.OuterXml;
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitAndFinishEETaskWithStorageProtectedInputAndClearOutput"), processor, xmlPreset, asset, TaskOptions.None);

            Assert.AreEqual(1, job.Tasks.Count);
            Assert.AreEqual(TaskOptions.None, job.Tasks[0].Options);
            Assert.IsNull(job.Tasks[0].InitializationVector);
            Assert.IsTrue(String.IsNullOrEmpty(job.Tasks[0].EncryptionKeyId));
            Assert.IsNull(job.Tasks[0].EncryptionScheme);
            Assert.IsNull(job.Tasks[0].EncryptionVersion);

            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);

            CloudMediaContext context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IJob job2 = context2.Jobs.Where(c => c.Id == job.Id).Single();

            Assert.AreEqual(1, job2.Tasks.Count);
            Assert.AreEqual(1, job2.Tasks[0].OutputAssets.Count);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [ExpectedException(typeof(DataServiceRequestException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldThrowTryingToDeleteJobInProcessingState()
        {
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldThrowTryingToDeleteJobInProcessingState"), processor, GetWamePreset(processor), asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Processing, (string id) => { });

            job.Delete();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDeleteJobInFinishedState()
        {
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldDeleteJobInFinishedState"), processor, GetWamePreset(processor), asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Finished, (string id) => { });
            job.Delete();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDeleteJobInCancelledState()
        {
            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldDeleteJobInCancelledState"), processor, GetWamePreset(processor), asset, TaskOptions.None);
            WaitForJob(job.Id, JobState.Processing, (string id) => { });
            job.Cancel();
            WaitForJob(job.Id, JobState.Canceled, (string id) => { });
            job.Delete();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\MP4 to Smooth Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\SmallMP41.mp4", "Media")]
        [Priority(0)]
        public void ShouldReceiveNotificationsForCompeletedJob()
        {
            string endPointAddress = Guid.NewGuid().ToString();
            CloudQueueClient client = CloudStorageAccount.Parse(WindowsAzureMediaServicesTestConfiguration.ClientStorageConnectionString).CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference(endPointAddress);
            queue.CreateIfNotExists();
            string endPointName = Guid.NewGuid().ToString();
            INotificationEndPoint notificationEndPoint = _mediaContext.NotificationEndPoints.Create(endPointName, NotificationEndPointType.AzureQueue, endPointAddress);
            Assert.IsNotNull(notificationEndPoint);

            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.DefaultMp4ToSmoothConfig);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpPackagerName);

            IJob job = _mediaContext.Jobs.Create("CreateJobWithNotificationSubscription");
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, configuration, TaskOptions.None);
            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("Output", AssetCreationOptions.None);

            job.JobNotificationSubscriptions.AddNew(NotificationJobState.All, notificationEndPoint);

            job.Submit();

            Assert.IsTrue(job.JobNotificationSubscriptions.Count > 0);

            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
            Thread.Sleep((int)TimeSpan.FromMinutes(5).TotalMilliseconds);

            Assert.IsNotNull(queue);
            Assert.IsTrue(queue.Exists());
            IEnumerable<CloudQueueMessage> messages = queue.GetMessages(10);
            Assert.IsTrue(messages.Any());
            Assert.AreEqual(4, messages.Count(), "Expecting to have 4 notifications messages");

            IJob lastJob = _mediaContext.Jobs.Where(j => j.Id == job.Id).FirstOrDefault();
            Assert.IsNotNull(lastJob);
            Assert.IsTrue(lastJob.JobNotificationSubscriptions.Count > 0);
            IJobNotificationSubscription lastJobNotificationSubscription = lastJob.JobNotificationSubscriptions.Where(n => n.NotificationEndPoint.Id == notificationEndPoint.Id).FirstOrDefault();
            Assert.IsNotNull(lastJobNotificationSubscription);
            INotificationEndPoint lastNotificationEndPoint = lastJobNotificationSubscription.NotificationEndPoint;
            Assert.IsNotNull(lastNotificationEndPoint);
            Assert.AreEqual(endPointName, lastNotificationEndPoint.Name);
            Assert.AreEqual(endPointAddress, lastNotificationEndPoint.EndPointAddress);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldUpdateJobPriorityWhenJobIsQueued()
        {
            const int newPriority = 3;

            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            //Create temp job to simuate queue when no reserved unit are allocated
            IJob tempJob = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("SubmitJobToCreateQueue"), processor, GetWamePreset(processor), asset, TaskOptions.None);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitJobAndUpdatePriorityWhenJobIsQueued"), processor, GetWamePreset(processor), asset, TaskOptions.None);

            WaitForJobStateAndUpdatePriority(job, JobState.Queued, newPriority);
            WaitForJob(job.Id, JobState.Finished, (string id) =>
                {
                    var finished = _mediaContext.Jobs.Where(c => c.Id == job.Id && c.Priority == newPriority).FirstOrDefault();
                    Assert.IsNotNull(finished);
                });
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void ShouldThrowTryingUpdateJobPriorityWhenJobIsProcessing()
        {
            const int newPriority = 3;

            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitJobAndUpdatePriorityWhenJobIsQueued"), processor, GetWamePreset(processor), asset, TaskOptions.None);
            try
            {
                WaitForJobStateAndUpdatePriority(job, JobState.Processing, newPriority);
            }
            catch (DataServiceRequestException ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains("Job's priority can only be changed if the job is in Queued state"));
                throw ex;
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void ShouldThrowTryingUpdateJobPriorityWhenJobIsFinished()
        {
            const int newPriority = 3;

            IMediaProcessor processor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IJob job = CreateAndSubmitOneTaskJob(_mediaContext, GenerateName("ShouldSubmitJobAndUpdatePriorityWhenJobIsQueued"), processor, GetWamePreset(processor), asset, TaskOptions.None);
            try
            {
                WaitForJobStateAndUpdatePriority(job, JobState.Finished, newPriority);
            }
            catch (DataServiceRequestException ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains("Job's priority can only be changed if the job is in Queued state"));
                throw ex;
            }
        }

        #region Helper Methods

        private IAsset CreateSmoothAsset()
        {
            var filePaths = new[] { WindowsAzureMediaServicesTestConfiguration.SmallIsm, WindowsAzureMediaServicesTestConfiguration.SmallIsmc, WindowsAzureMediaServicesTestConfiguration.SmallIsmv };
            return CreateSmoothAsset(filePaths);
        }

        private IAsset CreateSmoothAsset(string[] filePaths)
        {
            return CreateSmoothAsset(_mediaContext, filePaths, AssetCreationOptions.StorageEncrypted);
        }

        internal static IAsset CreateSmoothAsset(CloudMediaContext mediaContext, string[] filePaths, AssetCreationOptions options)
        {
            IAsset asset = mediaContext.Assets.Create(Guid.NewGuid().ToString(), options);
            IAccessPolicy policy = mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = mediaContext.Locators.CreateSasLocator(asset, policy);
            var blobclient = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = 5,
                ParallelTransferThreadCount = 5
            };


            foreach (string filePath in filePaths)
            {
                var info = new FileInfo(filePath);
                IAssetFile file = asset.AssetFiles.Create(info.Name);
                file.UploadAsync(filePath, blobclient, locator, CancellationToken.None).Wait();
                if (WindowsAzureMediaServicesTestConfiguration.SmallIsm == filePath)
                {
                    file.IsPrimary = true;
                    file.Update();
                }
            }
            return asset;
        }

        public static IJob CreateAndSubmitOneTaskJob(CloudMediaContext context, string name, IMediaProcessor mediaProcessor, string preset, IAsset asset, TaskOptions options)
        {
            IJob job = context.Jobs.Create(name);
            job.Priority = InitialJobPriority;
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, preset, options);
            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("Output asset", AssetCreationOptions.None);
            DateTime timebeforeSubmit = DateTime.UtcNow;
            job.Submit();
            Assert.AreEqual(1, job.Tasks.Count, "Job contains unexpected amount of tasks");
            Assert.AreEqual(1, job.InputMediaAssets.Count, "Job contains unexpected total amount of input assets");
            Assert.AreEqual(1, job.OutputMediaAssets.Count, "Job contains unexpected total amount of output assets");
            Assert.AreEqual(1, job.Tasks[0].InputAssets.Count, "job.Task[0] contains unexpected amount of input assets");
            Assert.AreEqual(1, job.Tasks[0].OutputAssets.Count, "job.Task[0] contains unexpected amount of output assets");
            Assert.IsFalse(String.IsNullOrEmpty(job.Tasks[0].InputAssets[0].Id), "Asset Id is Null or empty");
            Assert.IsFalse(String.IsNullOrEmpty(job.Tasks[0].OutputAssets[0].Id), "Asset Id is Null or empty");
            return job;
        }

        public static void WaitForJob(string jobId, JobState jobState, Action<string> verifyAction, Action<double> progressChangedAction = null)
        {
            DateTime start = DateTime.Now;
            while (true)
            {
                CloudMediaContext context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
                IJob job2 = context2.Jobs.Where(c => c.Id == jobId).Single();
                ITask jobtask = job2.Tasks.Where(t => t.State == JobState.Processing).FirstOrDefault();

                if (jobtask != null && jobtask.Progress > 0 && jobtask.Progress <= 100)
                {
                    if (progressChangedAction != null)
                    {
                        progressChangedAction(jobtask.Progress);
                    }
                }
                if (job2.State == jobState)
                {
                    verifyAction(jobId);
                    return;
                }
                if (job2.State == JobState.Error)
                {
                    StringBuilder str = new StringBuilder();
                    str.AppendFormat("Job should not fail - Current State = {0} Expected State = {1} jobId = {2}", job2.State, jobState, jobId);
                    str.AppendLine();
                    foreach (var task in job2.Tasks)
                    {
                        foreach (var error in task.ErrorDetails)
                        {
                            str.AppendFormat("Error Code: {0} ErrorMessage: {1}", error.Code, error.Message);
                            str.AppendLine();
                        }
                    }

                    throw new Exception(str.ToString());
                }

                if (DateTime.Now - start > TimeSpan.FromMinutes(JobTimeOutInMinutes))
                {
                    throw new Exception("Job Timed out - Current State " + job2.State.ToString() + " Expected State " + jobState + " jobId = " + jobId);
                }
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        public static IMediaProcessor GetEncoderMediaProcessor(CloudMediaContext context)
        {
            return GetMediaProcessor(context, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
        }

        public static IMediaProcessor GetMediaProcessor(CloudMediaContext context, string mpName)
        {
            IMediaProcessor mp = context.MediaProcessors.Where(c => c.Name == mpName).ToList().OrderByDescending(c => new Version(c.Version)).FirstOrDefault();

            if (mp == null)
            {
                throw new ArgumentException(string.Format("Media Processor {0} is not found", mpName), "mpName");
            }

            Trace.WriteLine(string.Format("Using media processor {0} Version {1}, ID {2}", mp.Name, mp.Version, mp.Id));
            return mp;
        }



        private string GenerateName(string name)
        {
            return NamePrefix + name;
        }

        /// <summary>
        /// Waits for expected job state and updates job priority.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="expectedJobState">Expected state of the job.</param>
        /// <param name="newPriority">The new priority.</param>
        private void WaitForJobStateAndUpdatePriority(IJob job, JobState expectedJobState, int newPriority)
        {
            WaitForJob(job.Id, expectedJobState, (string id) => { });

            job = _mediaContext.Jobs.Where(c => c.Id == job.Id).FirstOrDefault();
            Assert.IsNotNull(job);
            Assert.AreEqual(InitialJobPriority, job.Priority);
            job.Priority = newPriority;
            job.Update();

            job = _mediaContext.Jobs.Where(c => c.Id == job.Id).FirstOrDefault();
            Assert.IsNotNull(job);
            Assert.AreEqual(newPriority, job.Priority, "Job Priority is not matching expected value");
        }
        #endregion
    }
}