using System;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IOperation"/>.
    /// </summary>
    public class OperationBaseCollection
    {
        internal const string OperationSet = "Operations";
        private readonly CloudMediaContext _cloudMediaContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // todo: remove
        internal OperationBaseCollection(CloudMediaContext cloudMediaContext)
        {
            this._cloudMediaContext = cloudMediaContext;
        }

        /// <summary>
        /// Retrieves an operation by its Id.
        /// </summary>
        /// <param name="id">Id of the operation.</param>
        /// <returns>Operation.</returns>
        public IOperation GetOperation(string id)
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0}('{1}')", OperationSet, id), UriKind.Relative);
            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            var operation = new OperationData() { Id = id };
            dataContext.AttachTo(OperationSet, operation, Guid.NewGuid().ToString());
            dataContext.Execute<OperationData>(uri).Single();
            return operation;
        }
    }
}
