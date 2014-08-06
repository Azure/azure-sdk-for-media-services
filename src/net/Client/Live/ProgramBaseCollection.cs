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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IProgram"/>.
    /// </summary>
    public class ProgramBaseCollection : CloudBaseCollection<IProgram>
    {
        internal const string ProgramSet = "Programs";

        private readonly IChannel _parentChannel;
        private readonly Lazy<IQueryable<IProgram>> _programQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="MediaContextBase"/> instance.</param>
        internal ProgramBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            var dataContext = cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
			_programQuery = new Lazy<IQueryable<IProgram>>(() => dataContext.CreateQuery<IProgram, ProgramData>(ProgramSet));
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The cloud media context.</param>
        /// <param name="parentChannel">The parent <see cref="IChannel"/>.</param>
        internal ProgramBaseCollection(MediaContextBase cloudMediaContext, IChannel parentChannel)
            : this(cloudMediaContext)
        {
            _parentChannel = parentChannel;
        }

        /// <summary>
        /// Gets the queryable collection of programs.
        /// </summary>
        protected override IQueryable<IProgram> Queryable
        {
            get { return _parentChannel != null ? _programQuery.Value.Where(c => c.ChannelId == _parentChannel.Id) : _programQuery.Value; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Creates new Program.
        /// </summary>
        /// <param name="name">Name of the program.</param>
        /// <param name="archiveWindowLength">Archive window length.</param>
        /// <param name="assetId">Id of the asset where program content will be stored.</param>
        /// <returns>The created program.</returns>
        public IProgram Create(string name, TimeSpan archiveWindowLength, string assetId)
        {
            return Create(new ProgramCreationOptions(name, archiveWindowLength, assetId));
        }

        /// <summary>
        /// Creates new Program.
        /// </summary>
        /// <param name="name">Name of the program.</param>
        /// <param name="archiveWindowLength">Archive window length.</param>
        /// <param name="assetId">Id of the asset where program content will be stored.</param>
        /// <returns>The task to create the program.</returns>
        public Task<IProgram> CreateAsync(string name, TimeSpan archiveWindowLength, string assetId)
        {
            return CreateAsync(new ProgramCreationOptions(name, archiveWindowLength, assetId));
        }

        /// <summary>
        /// Creates new Program.
        /// </summary>
        /// <param name="options">Program creation options.</param>
        /// <returns>The created program.</returns>
        public IProgram Create(ProgramCreationOptions options)
        {
            return AsyncHelper.Wait(CreateAsync(options));
        }

        /// <summary>
        /// Asynchronously creates new Program.
        /// </summary>
        /// <param name="options">Program creation options.</param>
        /// <returns>The task to create the program.</returns>
        public Task<IProgram> CreateAsync(ProgramCreationOptions options)
        {
            if (string.IsNullOrEmpty(options.Name))
            {
                throw new ArgumentException(Resources.ErrorEmptyProgramName);
            }

            if (_parentChannel == null)
            {
                throw new InvalidOperationException(Resources.ErrorOrphanProgram);
            }

            var program = new ProgramData
            {
                Name = options.Name,
                Description = options.Description,
                ChannelId = _parentChannel.Id,
                AssetId = options.AssetId,
                ArchiveWindowLength = options.ArchiveWindowLength,
                ManifestName = options.ManifestName
            };

            program.SetMediaContext(MediaContext);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(ProgramSet, program);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(program))
                .ContinueWith<IProgram>(t =>
                {
                    t.ThrowIfFaulted();
                    return (ProgramData) t.Result.AsyncState;
                });
        }
   }
}
