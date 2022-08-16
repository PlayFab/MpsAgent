using PlayFab.MultiplayerModels;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.BuildTool
{
    public class BuildSettingsValidator
    {
        private readonly BuildSettings _settings;

        public BuildSettingsValidator(BuildSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException("Deployment settings cannot be null");
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(_settings.BuildName))
            {
                Console.WriteLine("Build name is required to create a build");
                return false;
            }

            if (_settings.MultiplayerServerCountPerVm < 0)
            {
                Console.WriteLine("Servers per machine must be greater than 0");
                return false;
            }

            bool vmSizeValidationSuccess = IsVmSizeValid(_settings.VmSize);

            bool regionsValidationSuccess = AreRegionsValid(_settings.RegionConfigurations);

            return vmSizeValidationSuccess && regionsValidationSuccess;
        }

        private bool IsVmSizeValid(string VmSize)
        {
            bool validVmSize;
            try
            {
                AzureVmSize vmSize = Enum.Parse<AzureVmSize>(VmSize);
                validVmSize = Enum.IsDefined(typeof(AzureVmSize), vmSize);
            }
            catch (Exception ex)
            {
                validVmSize = false;
                Console.WriteLine(ex.Message);
            }

            if (string.IsNullOrWhiteSpace(_settings.VmSize) || !validVmSize)
            {
                Console.WriteLine
                    ("Make sure you specified the right value for VmSize. " +
                    "Refer to this if you need: " +
                    "https://docs.microsoft.com/en-us/rest/api/playfab/multiplayer/multiplayer-server/create-build-with-custom-container?view=playfab-rest#azurevmsize"
                    );
                return false;
            }

            return true;
        }

        private bool AreRegionsValid(List<BuildRegionParams> regionDetails)
        {
            if (regionDetails?.Count > 0)
            {
                foreach (BuildRegionParams detail in regionDetails)
                {
                    bool isValidRegion;
                    try
                    {
                        AzureRegion region = Enum.Parse<AzureRegion>(detail.Region);
                        isValidRegion = Enum.IsDefined(typeof(AzureRegion), region);
                    }
                    catch(Exception ex)
                    {
                        isValidRegion = false;
                        Console.WriteLine(ex.Message);
                    }

                    if (string.IsNullOrEmpty(detail.Region) || !isValidRegion)
                    {
                        Console.WriteLine("Make sure you specified the right value for Region. Find reference here: " +
                            "https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.locationnames?view=azure-dotnet");
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

            Console.WriteLine("Region(s) must be specified for your game servers.");
            return false;
        }
    }
}
