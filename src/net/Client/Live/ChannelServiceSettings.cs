// Copyright 2012 Microsoft Corporation
// 
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

using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.Rest
{
    /// <summary>
    /// Describes Live channel settings.
    /// </summary>
    internal class ChannelServiceSettings
    {
        /// <summary>
        /// Gets or sets preview settings.
        /// </summary>
        public PreviewEndpointSettings Preview { get; set; }

        /// <summary>
        /// Gets or sets ingest settings.
        /// </summary>
        public IngestEndpointSettings Ingest { get; set; }

        /// <summary>
        /// Gets or sets input settings
        /// </summary>
        public InputSettings Input { get; set; }

        /// <summary>
        /// Gets or sets output settings
        /// </summary>
        public OutputSettings Output { get; set; }

        /// <summary>
        /// Creates an instance of ChannelServiceSettings class.
        /// </summary>
        public ChannelServiceSettings() { }

        /// <summary>
        /// Creates an instance of ChannelServiceSettings class from an instance of ChannelSettings.
        /// </summary>
        /// <param name="settings">Settings to copy into newly created instance.</param>
        public ChannelServiceSettings(ChannelSettings settings) 
        {
            if (settings == null) return;

            Preview = settings.Preview;
            Ingest = settings.Ingest;
            Output = settings.Output;

            if (settings.Input != null && settings.Input.FMp4FragmentDuration.HasValue)
            {
                Input = new InputSettings
                {
                    FMp4FragmentDuration = (UInt32) settings.Input.FMp4FragmentDuration.Value.Ticks
                };
            }
        }

        /// <summary>
        /// Casts ChannelServiceSettings to ChannelSettings.
        /// </summary>
        /// <param name="settings">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelSettings(ChannelServiceSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            var result = new ChannelSettings
            {
                Preview = settings.Preview, 
                Ingest = settings.Ingest,
                Output = settings.Output
            };

            if (settings.Input != null && settings.Input.FMp4FragmentDuration.HasValue)
            {
                result.Input = new Client.InputSettings
                {
                    FMp4FragmentDuration = TimeSpan.FromTicks(settings.Input.FMp4FragmentDuration.Value)
                };
            }

            return result;
        }
    }

    /// <summary>
    /// Describes Channel input settings
    /// </summary>
    internal class InputSettings
    {
        /// <summary>
        /// Gets or sets FMp4 fragment duration
        /// </summary>
        public UInt32? FMp4FragmentDuration { get; set; }
    }
}
