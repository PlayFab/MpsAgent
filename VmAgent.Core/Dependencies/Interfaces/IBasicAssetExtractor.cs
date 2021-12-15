using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Azure.Gaming.VmAgent.Core.Dependencies.Interfaces
{
    public interface IBasicAssetExtractor
    {
        void ExtractAssets(string assetFileName, string targetFolder);

        ProcessStartInfo GetProcessStartInfoForZip(string assetFileName, string targetFolder);

        ProcessStartInfo GetProcessStartInfoForTarOrGZip(string assetFileName, string targetFolder);
    }
}
