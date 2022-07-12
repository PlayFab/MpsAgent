using PlayFab.MultiplayerModels;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.DeploymentTool
{
    public class DeploymentSettingsValidator
    {
        //private readonly ISystemOperations _systemOperations;
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

            if (string.IsNullOrWhiteSpace(_settings.VmSize))
            {
                throw new Exception("VmSize was not specified");
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
                    if (string.IsNullOrEmpty(detail.Region))
                    {
                        Console.WriteLine("Region name unspecified");
                        return false;
                    }

                    if (string.IsNullOrEmpty(detail.VmSize.ToString()))
                    {
                        Console.WriteLine("Region details must contain a VmSize");
                        return false;
                    }

                    if (detail.MaxServers < 0)
                    {
                        Console.WriteLine("Max servers for a region should be greater than 0");
                        return false;
                    }

                    if (detail.MultiplayerServerCountPerVm < 0)
                    {
                        Console.WriteLine("Servers per machine should be greater than 0");
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
