//-----------------------------------------------------------------------
// <copyright file="PlayReadyPlayRight.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Configures the Play Right in the PlayReady license.  This right allows the client to play back the content
    /// and is required in license templates.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class PlayReadyPlayRight
    {
        /// <summary>
        /// Specifies the amount of time that the license is valid after the license is first used to play content.
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public TimeSpan? FirstPlayExpiration { get; set; }

        /// <summary>
        /// Configures the Serial Copy Management System (SCMS) in the license.  SCMS is a form of audio output protection.
        /// For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ScmsRestriction ScmsRestriction { get; set; }

        /// <summary>
        /// Configures Automatic Gain Control (AGC) and Color Stripe in the license.  These are a form of video output protection.
        /// For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AgcAndColorStripeRestriction AgcAndColorStripeRestriction { get; set; }

        /// <summary>
        /// Configures the Explicit Analog Television Output Restriction in the license.  This is a form of video output protection.
        /// For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ExplicitAnalogTelevisionRestriction ExplicitAnalogTelevisionOutputRestriction { get; set; }

        /// <summary>
        /// Enables the Digital Video Only Content Restriction in the license.  This is a form of video output protection
        /// which requires the player to output the video portion of the content over Digital Video Outputs.  For further 
        /// details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool DigitalVideoOnlyContentRestriction { get; set; }

        /// <summary>
        /// Enables the Image Constraint For Analog Component Video Restriction in the license.  This is a form of video output protection
        /// which requires the player constrain the resolution of the video portion of the content when outputting it over an Analog
        /// Component Video Output.  For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ImageConstraintForAnalogComponentVideoRestriction { get; set; }

        /// <summary>
        /// Enables the Image Constraint For Analog Computer Monitor Restriction in the license.  This is a form of video output protection
        /// which requires the player constrain the resolution of the video portion of the content when outputting it over an Analog
        /// Computer Monitor Output.  For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ImageConstraintForAnalogComputerMonitorRestriction { get; set; }

        /// <summary>
        /// This property configures Unknown output handling settings of the license.  These settings tell the PlayReady DRM runtime 
        /// how it should handle unknown video outputs.  For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UnknownOutputPassingOption AllowPassingVideoContentToUnknownOutput  { get; set; }

        /// <summary>
        /// Specifies the output protection level for uncompressed digital video.  Valid values are null, 100, 250, 270, and 300.
        /// When the property is set to null, the output protection level is not set in the license.  For further details on the meaning
        /// of the specific value see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? UncompressedDigitalVideoOpl
        { 
            get { return _uncompressedDigitalVideoOpl; }
            set
            {
                if ((value.HasValue) && (value != 100) && (value != 250) && (value != 270) && (value != 300))
                {
                    throw new ArgumentException(ErrorMessages.UncompressedDigitalVideoOplValueError);
                }

                _uncompressedDigitalVideoOpl = value;
            }
        }
        private int? _uncompressedDigitalVideoOpl;

        /// <summary>
        /// Specifies the output protection level for compressed digital video.  Valid values are null, 400, and 500.
        /// When the property is set to null, the output protection level is not set in the license.  For further details on the meaning
        /// of the specific value see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? CompressedDigitalVideoOpl
        {
            get { return _compressedDigitalVideoOpl; }
            set
            {
                if ((value.HasValue) && (value != 400) && (value != 500))
                {
                    throw new ArgumentException(ErrorMessages.CompressedDigitalVideoOplValueError);
                }

                _compressedDigitalVideoOpl = value;
            }
        }
        private int? _compressedDigitalVideoOpl;

        /// <summary>
        /// Specifies the output protection level for analog video.  Valid values are null, 100, 150, and 200.
        /// When the property is set to null, the output protection level is not set in the license.  For further details on the meaning
        /// of the specific value see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? AnalogVideoOpl
        {
            get { return _analogVideoOpl; }
            set
            {
                if ((value.HasValue) && (value != 100) && (value != 150) && (value != 200))
                {
                    throw new ArgumentException(ErrorMessages.AnalogVideoOplValueError);
                }

                _analogVideoOpl = value;
            }
        }
        private int? _analogVideoOpl;

        /// <summary>
        /// Specifies the output protection level for compressed digital audio.  Valid values are null, 100, 150, 200, 250, and 300.
        /// When the property is set to null, the output protection level is not set in the license.  For further details on the meaning
        /// of the specific value see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? CompressedDigitalAudioOpl
        {
            get { return _compressedDigitalAudioOpl; }
            set
            {
                if ((value.HasValue) && (value != 100) && (value != 150) && (value != 200) && (value != 250) && (value != 300))
                {
                    throw new ArgumentException(ErrorMessages.CompressedDigitalAudioOplValueError);
                }

                _compressedDigitalAudioOpl = value;
            }
        }
        private int? _compressedDigitalAudioOpl;

        /// <summary>
        /// Specifies the output protection level for uncompressed digital audio.  Valid values are 100, 150, 200, 250, and 300.
        /// When the property is set to null, the output protection level is not set in the license.  For further details on the meaning
        /// of the specific value see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? UncompressedDigitalAudioOpl
        {
            get { return _uncompressedDigitalAudioOpl; }
            set
            {
                if ((value.HasValue) && (value != 100) && (value != 150) && (value != 200) && (value != 250) && (value != 300))
                {
                    throw new ArgumentException(ErrorMessages.UncompressedDigitalAudioOplValueError);
                }

                _uncompressedDigitalAudioOpl = value;
            }
        }
        private int? _uncompressedDigitalAudioOpl;

    }
}
