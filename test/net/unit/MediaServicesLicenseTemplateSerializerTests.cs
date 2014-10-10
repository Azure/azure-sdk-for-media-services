//-----------------------------------------------------------------------
// <copyright file="MediaServicesLicenseTemplateSerializerTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class MediaServicesLicenseTemplateSerializerTests
    {
        [TestMethod]
        public void RoundTripTest()
        {
            PlayReadyLicenseResponseTemplate responseTemplate = new PlayReadyLicenseResponseTemplate();
            responseTemplate.ResponseCustomData = "This is my response custom data";
            PlayReadyLicenseTemplate licenseTemplate = new PlayReadyLicenseTemplate();
            responseTemplate.LicenseTemplates.Add(licenseTemplate);

            licenseTemplate.LicenseType = PlayReadyLicenseType.Persistent;
            licenseTemplate.BeginDate = DateTime.Now.AddHours(-1);
            licenseTemplate.ExpirationDate = DateTime.Now.AddDays(30).ToUniversalTime();

            licenseTemplate.PlayRight.CompressedDigitalAudioOpl = 300;
            licenseTemplate.PlayRight.CompressedDigitalVideoOpl = 400;
            licenseTemplate.PlayRight.UncompressedDigitalAudioOpl = 250;
            licenseTemplate.PlayRight.UncompressedDigitalVideoOpl = 270;
            licenseTemplate.PlayRight.AnalogVideoOpl = 100;
            licenseTemplate.PlayRight.AgcAndColorStripeRestriction = new AgcAndColorStripeRestriction(1);
            licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = UnknownOutputPassingOption.Allowed;
            licenseTemplate.PlayRight.ExplicitAnalogTelevisionOutputRestriction = new ExplicitAnalogTelevisionRestriction(0, true);
            licenseTemplate.PlayRight.ImageConstraintForAnalogComponentVideoRestriction = true;
            licenseTemplate.PlayRight.ImageConstraintForAnalogComputerMonitorRestriction = true;
            licenseTemplate.PlayRight.ScmsRestriction = new ScmsRestriction(2);

            string serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
            Assert.IsFalse(String.IsNullOrWhiteSpace(serializedTemplate));

            PlayReadyLicenseResponseTemplate responseTemplate2 = MediaServicesLicenseTemplateSerializer.Deserialize(serializedTemplate);
            Assert.IsNotNull(responseTemplate2);
        }

        [TestMethod]
        public void ValidateNonPersistentLicenseConstraints()
        {
            string serializedTemplate = null;
            PlayReadyLicenseResponseTemplate responseTemplate = new PlayReadyLicenseResponseTemplate();
            PlayReadyLicenseTemplate licenseTemplate = new PlayReadyLicenseTemplate();
            responseTemplate.LicenseTemplates.Add(licenseTemplate);

            // Part 1: Make sure we cannot set GracePeriod on a NonPersistent license
            licenseTemplate.LicenseType = PlayReadyLicenseType.Nonpersistent;
            licenseTemplate.GracePeriod = TimeSpan.FromDays(1);

            try
            {
                serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ErrorMessages.GracePeriodCannotBeSetOnNonPersistentLicense, ae.Message);
            }

            // Part 2: Make sure we cannot set a FirstPlayExpiration on a NonPersistent license.
            licenseTemplate.GracePeriod = null;
            licenseTemplate.PlayRight.FirstPlayExpiration = TimeSpan.FromDays(1);

            try
            {
                serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ErrorMessages.FirstPlayExpirationCannotBeSetOnNonPersistentLicense, ae.Message);
            }

            // Part 3: Make sure we cannot set a BeginDate on a NonPersistent license.
            licenseTemplate.PlayRight.FirstPlayExpiration = null;
            licenseTemplate.BeginDate = DateTime.UtcNow;

            try
            {
                serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ErrorMessages.BeginDateCannotBeSetOnNonPersistentLicense, ae.Message);
            }

            // Part 4: Make sure we cannot set an ExpirationDate on a NonPersistent license.
            licenseTemplate.BeginDate = null;
            licenseTemplate.ExpirationDate = DateTime.UtcNow;

            try
            {
                serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ErrorMessages.ExpirationCannotBeSetOnNonPersistentLicense, ae.Message);
            }
        }

        [TestMethod]
        public void DigitalVideoOnlyContentRestrictionAndAllowPassingVideoContentToUnknownOutputMutuallyExclusive()
        {
            string serializedTemplate = null;
            PlayReadyLicenseResponseTemplate responseTemplate = new PlayReadyLicenseResponseTemplate();
            PlayReadyLicenseTemplate licenseTemplate = new PlayReadyLicenseTemplate();
            responseTemplate.LicenseTemplates.Add(licenseTemplate);

            // Part 1: Make sure we cannot set DigitalVideoOnlyContentRestriction to true if 
            //         UnknownOutputPassingOption.Allowed is set
            licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = UnknownOutputPassingOption.Allowed;
            licenseTemplate.PlayRight.DigitalVideoOnlyContentRestriction = true;

            try
            {
                serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ErrorMessages.DigitalVideoOnlyMutuallyExclusiveWithPassingToUnknownOutputError, ae.Message);
            }

            // Part 2: Make sure we cannot set UnknownOutputPassingOption.AllowedWithVideoConstriction
            //         if DigitalVideoOnlyContentRestriction is true
            licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = UnknownOutputPassingOption.AllowedWithVideoConstriction;

            try
            {
                serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
                Assert.Fail("Expected ArgumentException");
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ErrorMessages.DigitalVideoOnlyMutuallyExclusiveWithPassingToUnknownOutputError, ae.Message);
            }

            // Part 3: Make sure we can set DigitalVideoOnlyContentRestriction to true if 
            //         UnknownOutputPassingOption.NotAllowed is set
            licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = UnknownOutputPassingOption.NotAllowed;
            licenseTemplate.PlayRight.DigitalVideoOnlyContentRestriction = true;

            serializedTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
            Assert.IsNotNull(serializedTemplate);

            Assert.IsNotNull(MediaServicesLicenseTemplateSerializer.Deserialize(serializedTemplate));
        }

        [TestMethod]
        public void KnownGoodInputTest()
        {
            string serializedTemplate = "<PlayReadyLicenseResponseTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1\"><LicenseTemplates><PlayReadyLicenseTemplate><AllowTestDevices>false</AllowTestDevices><BeginDate i:nil=\"true\" /><ContentKey i:type=\"ContentEncryptionKeyFromHeader\" /><ContentType>Unspecified</ContentType><ExpirationDate i:nil=\"true\" /><LicenseType>Nonpersistent</LicenseType><PlayRight><AgcAndColorStripeRestriction><ConfigurationData>1</ConfigurationData></AgcAndColorStripeRestriction><AllowPassingVideoContentToUnknownOutput>Allowed</AllowPassingVideoContentToUnknownOutput><AnalogVideoOpl>100</AnalogVideoOpl><CompressedDigitalAudioOpl>300</CompressedDigitalAudioOpl><CompressedDigitalVideoOpl>400</CompressedDigitalVideoOpl><DigitalVideoOnlyContentRestriction>false</DigitalVideoOnlyContentRestriction><ExplicitAnalogTelevisionOutputRestriction><BestEffort>true</BestEffort><ConfigurationData>0</ConfigurationData></ExplicitAnalogTelevisionOutputRestriction><ImageConstraintForAnalogComponentVideoRestriction>true</ImageConstraintForAnalogComponentVideoRestriction><ImageConstraintForAnalogComputerMonitorRestriction>true</ImageConstraintForAnalogComputerMonitorRestriction><ScmsRestriction><ConfigurationData>2</ConfigurationData></ScmsRestriction><UncompressedDigitalAudioOpl>250</UncompressedDigitalAudioOpl><UncompressedDigitalVideoOpl>270</UncompressedDigitalVideoOpl></PlayRight></PlayReadyLicenseTemplate></LicenseTemplates><ResponseCustomData>This is my response custom data</ResponseCustomData></PlayReadyLicenseResponseTemplate>";

            PlayReadyLicenseResponseTemplate responseTemplate2 = MediaServicesLicenseTemplateSerializer.Deserialize(serializedTemplate);
            Assert.IsNotNull(responseTemplate2);
        }

        [TestMethod]
        public void KnownGoodInputMinimalLicenseTest()
        {
            string serializedTemplate = "<PlayReadyLicenseResponseTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1\"><LicenseTemplates><PlayReadyLicenseTemplate><ContentKey i:type=\"ContentEncryptionKeyFromHeader\" /><PlayRight /></PlayReadyLicenseTemplate></LicenseTemplates></PlayReadyLicenseResponseTemplate>";

            PlayReadyLicenseResponseTemplate responseTemplate2 = MediaServicesLicenseTemplateSerializer.Deserialize(serializedTemplate);
            Assert.IsNotNull(responseTemplate2);
        }

        [TestMethod]
        public void InputMissingContentKeyShouldThrowArgumentException()
        {
            string serializedTemplate = "<PlayReadyLicenseResponseTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1\"><LicenseTemplates><PlayReadyLicenseTemplate><PlayRight /></PlayReadyLicenseTemplate></LicenseTemplates></PlayReadyLicenseResponseTemplate>";

            try
            {
                PlayReadyLicenseResponseTemplate responseTemplate2 = MediaServicesLicenseTemplateSerializer.Deserialize(serializedTemplate);
                Assert.Fail("Should throw an ArgumentException");
            }
            catch (SerializationException e)
            {
                e.Message.Contains("ContentKey");
            }
        }

        [TestMethod]
        public void InputMissingPlayRightShouldThrowArgumentException()
        {
            string serializedTemplate = "<PlayReadyLicenseResponseTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1\"><LicenseTemplates><PlayReadyLicenseTemplate><ContentKey i:type=\"ContentEncryptionKeyFromHeader\" /></PlayReadyLicenseTemplate></LicenseTemplates></PlayReadyLicenseResponseTemplate>";

            try
            {
                PlayReadyLicenseResponseTemplate responseTemplate2 = MediaServicesLicenseTemplateSerializer.Deserialize(serializedTemplate);
                Assert.Fail("Should throw an ArgumentException");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains(ErrorMessages.PlayReadyPlayRightRequired));
            }
        }

        [TestMethod]
        public void InputMissingLicenseTemplatesShouldThrowArgumentException()
        {
            string serializedTemplate = "<PlayReadyLicenseResponseTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1\"><LicenseTemplates></LicenseTemplates></PlayReadyLicenseResponseTemplate>";

            try
            {
                PlayReadyLicenseResponseTemplate responseTemplate2 = MediaServicesLicenseTemplateSerializer.Deserialize(serializedTemplate);
                Assert.Fail("Should throw an ArgumentException");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.Contains(ErrorMessages.AtLeastOneLicenseTemplateRequired));
            }
        }

        [TestMethod]
        public void ScmsRestrictionConfigurationDataValidationTest()
        {
            byte[] validConfigurationValues = new byte[] { 0, 1, 2, 3 };
            byte[] invalidConfigurationValues = new byte[] {255, 128, 4, 5, 15};

            foreach (byte configurationData in validConfigurationValues)
            {
                ScmsRestriction restriction = new ScmsRestriction(configurationData);
            }

            foreach (byte configurationData in invalidConfigurationValues)
            {
                try
                {
                    ScmsRestriction restriction = new ScmsRestriction(configurationData);
                    Assert.Fail("Invalid configuration data accepted");
                }
                catch (ArgumentException ae)
                {
                    Assert.AreEqual(ErrorMessages.InvalidTwoBitConfigurationData, ae.Message);
                }
            }
        }

        [TestMethod]
        public void ValidateOutputProtectionLevelValueChecks()
        {
            //  From the PlayReady Compliance Rules for issuing PlayReady Licenses.
            //
            //                         Table 6.6: Allowed Output Protection Level Values
            //
            //                      Field	                                    Allowed Values
            //
            //  Minimum Compressed Digital Audio Output Protection Level	100, 150, 200, 250, 300
            //  Minimum Uncompressed Digital Audio Output Protection Level	100, 150, 200, 250, 300
            //  Minimum Compressed Digital Video Output Protection Level	400, 500
            //  Minimum Uncompressed Digital Video Output Protection Level	100, 250, 270, 300
            //  Minimum Analog Television Output Protection Level	        100, 150, 200
            //

            bool[] expectedResult = null;

            // First check null, which all of the Opls values support.  Setting Opl values is optional
            // and null is the way the user signals that they do not want to set a value.
            bool[] currentResult = SetOutputProtectionLevelValues(null);
            Assert.IsFalse(currentResult.Any(c => c == false), "null result didn't match expectations");

            for (int i = 0; i <= 550; i += 10)
            {
                currentResult = SetOutputProtectionLevelValues(i);

                switch (i)
                { 
                    case 100:
                        expectedResult = new bool[] {true, true, false, true, true };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "100 result didn't match expectations");
                        break;
                    case 150:
                        expectedResult = new bool[] {true, true, false, false, true };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "150 result didn't match expectations");
                        break;
                    case 200:
                        expectedResult = new bool[] {true, true, false, false, true };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "200 result didn't match expectations");
                        break;
                    case 250:
                        expectedResult = new bool[] {true, true, false, true, false };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "250 result didn't match expectations");
                        break;
                    case 270:
                        expectedResult = new bool[] { false, false, false, true, false };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "270 result didn't match expectations");
                        break;
                    case 300:
                        expectedResult = new bool[] { true, true, false, true, false };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "300 result didn't match expectations");
                        break;
                    case 400:
                        expectedResult = new bool[] { false, false, true, false, false };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "400 result didn't match expectations");
                        break;
                    case 500:
                        expectedResult = new bool[] { false, false, true, false, false };
                        Assert.IsTrue(currentResult.SequenceEqual(expectedResult), "500 result didn't match expectations");
                        break;
                    default:
                        // These values should always return false for all types
                        string message = string.Format("{0} result didn't match expectations", i);
                        Assert.IsFalse(currentResult.Any(c => c == true), message);
                        break;
                }

            }
        }

        private bool[] SetOutputProtectionLevelValues(int? valueToSet)
        {
            PlayReadyPlayRight playRight = new PlayReadyPlayRight();
            bool[] returnValue = new bool[5];

            try
            {
                playRight.CompressedDigitalAudioOpl = valueToSet;
                returnValue[0] = true;
            }
            catch (ArgumentException ae)
            {
                if (ae.Message != ErrorMessages.CompressedDigitalAudioOplValueError)
                {
                    throw;
                }
            }

            try
            {
                playRight.UncompressedDigitalAudioOpl = valueToSet;
                returnValue[1] = true;
            }
            catch (ArgumentException ae)
            {
                if (ae.Message != ErrorMessages.UncompressedDigitalAudioOplValueError)
                {
                    throw;
                }
            }

            try
            {
                playRight.CompressedDigitalVideoOpl = valueToSet;
                returnValue[2] = true;
            }
            catch (ArgumentException ae)
            {
                if (ae.Message != ErrorMessages.CompressedDigitalVideoOplValueError)
                {
                    throw;
                }
            }

            try
            {
                playRight.UncompressedDigitalVideoOpl = valueToSet;
                returnValue[3] = true;
            }
            catch (ArgumentException ae)
            {
                if (ae.Message != ErrorMessages.UncompressedDigitalVideoOplValueError)
                {
                    throw;
                }
            }

            try
            {
                playRight.AnalogVideoOpl = valueToSet;
                returnValue[4] = true;
            }
            catch (ArgumentException ae)
            {
                if (ae.Message != ErrorMessages.AnalogVideoOplValueError)
                {
                    throw;
                }
            }

            return returnValue;
        }
    }
}
