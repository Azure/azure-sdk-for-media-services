using System;
using System.IO;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Common
{
    public class ErrorDetectionStrategyForRefreshToken : SaveChangesErrorDetectionStrategy
    {
        //This ErrorDetectionStategy was created to check if retrypolicy associated with it is invoked.
        //Setting Invoked to make sure this is used. (for testing purposes.)
        public Boolean Invoked { get; set; }
        protected override bool CheckIsTransient(Exception ex)
        {
            Invoked = true;
            return false;

        }
    }
}