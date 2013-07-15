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
using System.Data.Services.Client;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IProgram"/>.
    /// </summary>
    public class ProgramBaseCollection : CloudBaseCollection<IProgram>
    {
        internal const string ProgramSet = "Programs";
        private readonly CloudMediaContext _cloudMediaContext;
        private readonly Lazy<IQueryable<IProgram>> _programQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal ProgramBaseCollection(CloudMediaContext cloudMediaContext)
        {
            this._cloudMediaContext = cloudMediaContext;

            this.DataContextFactory = this._cloudMediaContext.DataContextFactory;
            var dataContext = cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            this._programQuery = new Lazy<IQueryable<IProgram>>(() => dataContext.CreateQuery<ProgramData>(ProgramSet));
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The cloud media context.</param>
        /// <param name="parentChannel">The parent <see cref="IChannel"/>.</param>
        internal ProgramBaseCollection(CloudMediaContext cloudMediaContext, IChannel parentChannel)
            : this(cloudMediaContext)
        {
            _parentChannel = parentChannel;
        }

        /// <summary>
        /// Gets the queryable collection of programs.
        /// </summary>
        protected override IQueryable<IProgram> Queryable
        {
            get { return _parentChannel != null ? this._programQuery.Value.Where(c => c.ChannelId == _parentChannel.Id) : this._programQuery.Value; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Creates new Program.
        /// </summary>
        /// <param name="name">Name of the program.</param>
        /// <param name="description">Program description.</param>
        /// <param name="enableArchive">True if the program must be archived.</param>
        /// <param name="lookAheadFragments">Number of look ahead fragments.</param>
        /// <param name="dvrWindowLength">Length of the DVR window. 
        /// Set to StreamingConstants.InfiniteDvrLenth for infinite DVR.</param>
        /// <param name="estimatedDuration">Estimated duration of the program.</param>
        /// <param name="assetId">Id of the asset where program content will be stored.</param>
        /// <returns>The created program.</returns>
        public IProgram Create(
            string name,
            string description,
            bool enableArchive,
            int lookAheadFragments,
            TimeSpan? dvrWindowLength,
            TimeSpan estimatedDuration,
            string assetId)
        {
            return AsyncHelper.Wait(this.CreateAsync(
                name,
                description,
                enableArchive,
                lookAheadFragments,
                dvrWindowLength,
                estimatedDuration,
                assetId));
        }

        /// <summary>
        /// Creates new Program.
        /// </summary>
        /// <param name="name">Name of the program.</param>
        /// <param name="enableArchive">True if the program must be archived.</param>
        /// <param name="lookAheadFragments">Number of look ahead fragments.</param>
        /// <param name="dvrWindowLength">Length of the DVR window. 
        /// Set to StreamingConstants.InfiniteDvrLenth for infinite DVR.</param>
        /// <param name="estimatedDuration">Estimated duration of the program.</param>
        /// <param name="assetId">Id of the asset where program content will be stored.</param>
        /// <returns>The created program.</returns>
        public IProgram Create(
            string name,
            bool enableArchive,
            int lookAheadFragments,
            TimeSpan? dvrWindowLength,
            TimeSpan estimatedDuration,
            string assetId)
        {
            return Create(
                name,
                null,
                enableArchive,
                lookAheadFragments,
                dvrWindowLength,
                estimatedDuration,
                assetId);
        }

        /// <summary>
        /// Creates new Program.
        /// </summary>
        /// <param name="name">Name of the program.</param>
        /// <param name="enableArchive">True if the program must be archived.</param>
        /// <param name="lookAheadFragments">Number of look ahead fragments.</param>
        /// <param name="dvrWindowLength">Length of the DVR window. 
        /// Set to StreamingConstants.InfiniteDvrLenth for infinite DVR.</param>
        /// <param name="estimatedDuration">Estimated duration of the program.</param>
        /// <param name="assetId">Id of the asset where program content will be stored.</param>
        /// <returns>The created program.</returns>
        public Task<IProgram> CreateAsync(
            string name,
            bool enableArchive,
            int lookAheadFragments,
            TimeSpan? dvrWindowLength,
            TimeSpan estimatedDuration,
            string assetId)
        {
            return CreateAsync(
                name,
                null,
                enableArchive,
                lookAheadFragments,
                dvrWindowLength,
                estimatedDuration,
                assetId);
        }

        /// <summary>
        /// Asynchronously creates new Program.
        /// </summary>
        /// <param name="name">Name of the program.</param>
        /// <param name="description">Program description.</param>
        /// <param name="enableArchive">True if the program must be archived.</param>
        /// <param name="lookAheadFragments">Number of look ahead fragments.</param>
        /// <param name="dvrWindowLength">Length of the DVR window. 
        /// Set to StreamingConstants.InfiniteDvrLenth for infinite DVR.</param>
        /// <param name="estimatedDuration">Estimated duration of the program.</param>
        /// <param name="assetId">Id of the asset where program content will be stored.</param>
        /// <returns>The created program.</returns>
        public Task<IProgram> CreateAsync(
            string name,
            string description,
            bool enableArchive,
            int lookAheadFragments,
            TimeSpan? dvrWindowLength, 
            TimeSpan estimatedDuration,
            string assetId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyProgramName);
            }

            if (_parentChannel == null)
            {
                throw new InvalidOperationException(Resources.ErrorOrphanProgram);
            }

            var program = new ProgramData
            {
                ChannelId = _parentChannel.Id,
                AssetId = assetId,
                Description = description,
                EstimatedDurationSeconds = (int)estimatedDuration.TotalSeconds,
                EnableArchive = enableArchive,
                LookAheadFragments = lookAheadFragments,
                Name = name
            };

            if (dvrWindowLength.HasValue)
            {
                program.DvrWindowLengthSeconds = (int)dvrWindowLength.Value.TotalSeconds;
            }

            program.InitCloudMediaContext(this._cloudMediaContext);

            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AddObject(ProgramSet, program);

            return dataContext
                .SaveChangesAsync(program)
                .ContinueWith<IProgram>(t =>
                    {
                        t.ThrowIfFaulted();
                        return (ProgramData)t.AsyncState;
                    });
        }

        private readonly IChannel _parentChannel;
   }
}
