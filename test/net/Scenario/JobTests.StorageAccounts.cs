//-----------------------------------------------------------------------
// <copyright file="JobTests.StorageAccounts.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    public partial class JobTests
    {

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void ShouldThrowSubmittingJobWhenNonexistingStorageSpecifiedForOutPut()
        {
            try
            {
                IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
                IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
                string name = GenerateName("Job 1");
                IJob job = _mediaContext.Jobs.Create(name);
                ITask task = job.Tasks.AddNew("Task1", mediaProcessor, GetWamePreset(mediaProcessor), TaskOptions.None);
                task.InputAssets.Add(asset);
                task.OutputAssets.AddNew("Output asset", Guid.NewGuid().ToString(), AssetCreationOptions.None);
                job.Submit();
            }
            catch (DataServiceRequestException ex)
            {
                Assert.IsTrue(ex.Response.First().Error.Message.Contains("Cannot find the storage account"));
                throw;
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSubmitJobWhereOutPutInDefaultStorage()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("Job 1");
            IJob job = _mediaContext.Jobs.Create(name);
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, GetWamePreset(mediaProcessor), TaskOptions.None);
            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("Output asset", _mediaContext.DefaultStorageAccount.Name, AssetCreationOptions.None);
            job.Submit();
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSubmitJobWhereOutPutInNoneDefaultStorage()
        {
            var nondefault = _mediaContext.StorageAccounts.Where(c => c.IsDefault == false).FirstOrDefault();
            //This test need to be executed when media account multiple storage account associated with it. 
            if (nondefault == null)
            {
                return;
            }
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("Job 1");
            IJob job = _mediaContext.Jobs.Create(name);
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, GetWamePreset(mediaProcessor), TaskOptions.None);
            task.InputAssets.Add(asset);
            var outputAsset = task.OutputAssets.AddNew("Output asset", nondefault.Name, AssetCreationOptions.None);
            job.Submit();
            Assert.AreEqual(nondefault.Name, outputAsset.StorageAccountName, "Storage account are not matching");
            WaitForJob(job.Id, JobState.Finished, VerifyAllTasksFinished);

            var refreshed = _mediaContext.Jobs.Where(c => c.Id == job.Id).FirstOrDefault();
            Assert.IsNotNull(refreshed);
            Assert.AreEqual(1, refreshed.Tasks.Count, "Number of Tasks in job is not matching");
            Assert.AreEqual(1, refreshed.Tasks[0].OutputAssets.Count, "Number of output assets in job is not matching");
            Assert.AreEqual(nondefault.Name, refreshed.Tasks[0].OutputAssets[0].StorageAccountName, "Storage account name in output assset is not matching");

        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSaveJobAsTemplateAndCreateNewJobwithItWhereOutPutInNoneDefaultStorage()
        {
            var nondefault = _mediaContext.StorageAccounts.Where(c => c.IsDefault == false).FirstOrDefault();

            //This test need to be executed when media account multiple storage account associated with it. 
            if (nondefault == null)
            {
                return;
            }

            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            string name = GenerateName("Job 1");
            IJob job = _mediaContext.Jobs.Create(name);
            ITask task = job.Tasks.AddNew("Task1", mediaProcessor, GetWamePreset(mediaProcessor), TaskOptions.None);
            task.InputAssets.Add(asset);
            var outputAsset = task.OutputAssets.AddNew("Output asset", nondefault.Name, AssetCreationOptions.None);
            job.Submit();
            var template = job.SaveAsTemplate("JobTests.StorageAccounts.cs_" + Guid.NewGuid().ToString());
            string newJobName = GenerateName("Job from template with non default storage account");
            var newJob = _mediaContext.Jobs.Create(newJobName, template, new[] { asset });
            newJob.Submit();
            WaitForJob(newJob.Id, JobState.Finished, VerifyAllTasksFinished);
            newJob = _mediaContext.Jobs.Where(c => c.Id == newJob.Id).FirstOrDefault();
            Assert.AreEqual(nondefault.Name, newJob.Tasks[0].OutputAssets[0].StorageAccountName, "Storage account name in output assset is not matching");


        }

    }
}