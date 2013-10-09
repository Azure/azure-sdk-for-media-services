using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Client;
using System.Collections;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public interface IMediaDataServiceResponse : IEnumerable<OperationResponse>, IEnumerable
    {
        /// <summary>
        /// The headers from an HTTP response associated with a batch request.
        /// </summary>
        IDictionary<string, string> BatchHeaders { get; }

        /// <summary>
        /// The status code from an HTTP response associated with a batch request.
        /// </summary>
        int BatchStatusCode { get; }

        /// <summary>
        /// Gets a Boolean value that indicates whether the response contains multiple results.
        /// </summary>
        bool IsBatchResponse { get; }

        /// <summary>
        /// Preserves async state destroyed by retry mechanism.
        /// </summary>
        object AsyncState { get; set; }
    }
}
