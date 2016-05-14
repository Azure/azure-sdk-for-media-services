//-----------------------------------------------------------------------
// <copyright file="ComponentMonitoringSetting.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.ComponentModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// The monitoring settings for a component.
    /// </summary>
    public class ComponentMonitoringSetting : IComponentMonitoringSetting
    {
        /// <summary>
        /// Gets or sets the monitoring component in enum type.
        /// </summary>
        MonitoringComponent IComponentMonitoringSetting.Component { get; set; }

        /// <summary>
        /// Gets or sets the monitoring component in enum type.
        /// </summary>
        MonitoringLevel IComponentMonitoringSetting.Level { get; set; }

        /// <summary>
        /// Gets or sets the monitoring component in string type.
        /// when setting the value, the parameter should be a string from MonitoringComponent (e.g. MonitoringComponent.Channel.ToString()).
        /// </summary>
        public string Component
        {
            get { return ((IComponentMonitoringSetting) this).Component.ToString(); }
            set
            {
                if (!Enum.IsDefined(typeof(MonitoringComponent), value))
                {
                    throw new InvalidEnumArgumentException("Component value is not a member of the MonitoringComponent enumeration");
                }
                ((IComponentMonitoringSetting) this).Component = (MonitoringComponent) Enum.Parse(typeof (MonitoringComponent), value);
            }
        }


        /// <summary>
        /// Gets or sets the monitoring level in string type.
        /// when setting the value, the parameter should be a string from MonitoringLevel (e.g MonitoringLevel.Normal.ToString()).
        /// </summary>
        public string Level
        {
            get { return ((IComponentMonitoringSetting) this).Level.ToString(); }
            set
            {
                if (!Enum.IsDefined(typeof(MonitoringLevel), value))
                {
                    throw new InvalidEnumArgumentException("Level value is not a member of the MonitoringLevel enumeration");
                }
                ((IComponentMonitoringSetting) this).Level = (MonitoringLevel) Enum.Parse(typeof (MonitoringLevel), value);
            }
        }

        /// <summary>
        /// Default constructor of ComponentMonitoringSetting
        /// </summary>
        public ComponentMonitoringSetting() { }

        /// <summary>
        /// Constructor of ComponentMonitoringSetting
        /// </summary>
        /// <param name="component">The type of monitoring component</param>
        /// <param name="level">The type of monitoring level</param>
        public ComponentMonitoringSetting(MonitoringComponent component, MonitoringLevel level)
        {
            ((IComponentMonitoringSetting) this).Component = component;
            ((IComponentMonitoringSetting) this).Level = level;
        }
    }
}
