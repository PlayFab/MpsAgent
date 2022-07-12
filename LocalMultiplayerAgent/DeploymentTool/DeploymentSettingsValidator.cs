using PlayFab.MultiplayerModels;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool
{
    public class DeploymentSettingsValidator
    {
        private readonly DeploymentSettings _settings;

        public DeploymentSettingsValidator(DeploymentSettings settings)
        {
            _settings = settings ?? throw new ArgumentException("Deployment settings cannot be null");
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(_settings.BuildName))
            {
                throw new Exception("Build name is required to create a build");
            }

            AzureVmSize vmSize = Enum.Parse<AzureVmSize>(_settings.VmSize);
            bool validVmSize = Enum.IsDefined(typeof(AzureVmSize), vmSize);

            if (string.IsNullOrWhiteSpace(_settings.VmSize) || !validVmSize)
            {
                throw new Exception("Make sure you specified the right value for VmSize");
            }

            if (string.IsNullOrWhiteSpace(_settings.OSPlatform))
            {
                throw new Exception("OSPlatform must be specified");
            }


            bool assetsValidationSuccess = AreAssetsValid(_settings.AssetFileNames);
            bool regionsValidationSuccess = AreRegionsValid(_settings.RegionConfigurations);

            return assetsValidationSuccess && regionsValidationSuccess;

        }

        private bool AreAssetsValid(List<string> assetFileNames)
        {
            if (assetFileNames?.Count > 0)
            {
                foreach (string fileName in assetFileNames)
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        Console.WriteLine("Asset file name must be specified");
                        return false;
                    }
                }

                return true;
            }

            Console.WriteLine("Assets must be specified for game servers running on Windows.");
            return false;

        }

        private bool AreRegionsValid(List<BuildRegionParams> regionDetails)
        {
            if (regionDetails?.Count > 0)
            {
                foreach (BuildRegionParams detail in regionDetails)
                {
                    AzureRegion region = Enum.Parse<AzureRegion>(detail.Region);
                    bool isValidRegion = Enum.IsDefined(typeof(AzureRegion), region);

                    if (string.IsNullOrEmpty(detail.Region) || !isValidRegion)
                    {
                        Console.WriteLine("Make sure you specified the right value for Region");
                        return false;
                    }

                    if (detail.MaxServers < 0)
                    {
                        Console.WriteLine($"Max servers for a region {detail.Region} should be greater than 0");
                        return false;
                    }

                    if (!string.IsNullOrEmpty(detail.VmSize.ToString()) && Enum.IsDefined(typeof(AzureVmSize), detail.VmSize))
                    {
                        Console.WriteLine($"Regional override in {detail.Region} must contain the right VmSize");
                        return false;
                    }

                    if (detail.MultiplayerServerCountPerVm != null && detail.MultiplayerServerCountPerVm < 0)
                    {
                        Console.WriteLine($"Regional override in {detail.Region} must have servers per machine greater than 0");
                        return false;
                    }
                }

                return true;
            }

            Console.WriteLine("Assets must be specified for game servers running on Windows.");
            return false;

        }
    }
}
