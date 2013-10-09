using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Client;
using System.Collections;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public class MediaDataServiceResponse : IMediaDataServiceResponse
    {
        public MediaDataServiceResponse(DataServiceResponse response)
        {
            _response = response;
        }

        /// <summary>
        /// The headers from an HTTP response associated with a batch request.
        /// </summary>
        public IDictionary<string, string> BatchHeaders { get { return _response.BatchHeaders; } }

        /// <summary>
        /// The status code from an HTTP response associated with a batch request.
        /// </summary>
        public int BatchStatusCode { get { return _response.BatchStatusCode; } }

        /// <summary>
        /// Gets a Boolean value that indicates whether the response contains multiple results.
        /// </summary>
        public bool IsBatchResponse { get { return _response.IsBatchResponse; } }

        /// <summary>
        /// Gets an enumerator that enables retrieval of responses to operations being
        /// tracked by System.Data.Services.Client.OperationResponse objects within the
        /// System.Data.Services.Client.DataServiceResponse. 
        /// </summary>
        /// <returns>An enumerator over the response received from the service.</returns>
        public IEnumerator<OperationResponse> GetEnumerator() { return _response.GetEnumerator(); }

        /// <summary>
        /// Gets an enumerator that enables retrieval of responses to operations being
        /// tracked by System.Data.Services.Client.OperationResponse objects within the
        /// System.Data.Services.Client.DataServiceResponse. 
        /// </summary>
        /// <returns>An enumerator over the response received from the service.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return _response.GetEnumerator(); }

        /// <summary>
        /// Preserves async state destroyed by retry mechanism.
        /// </summary>
        public object AsyncState { get; set; }

        private DataServiceResponse _response;
    }
}
