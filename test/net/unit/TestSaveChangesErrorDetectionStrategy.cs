using System;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Common
{
    public class TestSaveChangesErrorDetectionStrategy : SaveChangesErrorDetectionStrategy
    {
        //created a new detectionstrategy for savecanges to add timeoutexception in the transient exception list.
        protected override bool CheckIsTransient(Exception ex)
        {
            if (IsIOException(ex))
            {
                return true;
            }

            return base.CheckIsTransient(ex);

        }
        protected bool IsIOException(Exception ex)
        {
            return (ex is IOException || (ex.FindInnerException<IOException>() != null));
        }
    }
}
