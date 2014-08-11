using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public enum AssetType
    {
        Unknown,
        MP4,
        MultiBitrateMP4,
        SmoothStreaming,
        MediaServicesHLS // Is this the best name for this?
    }
}
