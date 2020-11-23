// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    public static class LegacyAzureRegionHelper
    {
        public static string GetRegionString(string region)
        {
            switch (region)
            {
                case "ChinaEast":
                    return "China East";
                case "ChinaEast2":
                    return "China East 2";
                case "ChinaNorth":
                    return "China North";
                case "ChinaNorth2":
                    return "China North 2";
                case "FranceCentral":
                    return "France Central";
                case "FranceSouth":
                    return "France South";
                case "KoreaCentral":
                    return "Korea Central";
                case "KoreaSouth":
                    return "Korea South";
                case "WestCentralUs":
                    return "West Central US";
                case "WestUs2":
                    return "West US 2";
                case "WestUs":
                    return "West US";
                case "EastUs":
                    return "East US";
                case "EastUs2":
                    return "East US 2";
                case "EastAsia":
                    return "East Asia";
                case "SoutheastAsia":
                    return "Southeast Asia";
                case "NorthEurope":
                    return "North Europe";
                case "WestEurope":
                    return "West Europe";
                case "NorthCentralUs":
                    return "North Central US";
                case "CentralUs":
                    return "Central US";
                case "SouthCentralUs":
                    return "South Central US";
                case "JapanEast":
                    return "Japan East";
                case "JapanWest":
                    return "Japan West";
                case "BrazilSouth":
                    return "Brazil South";
                case "AustraliaEast":
                    return "Australia East";
                case "AustraliaSoutheast":
                    return "Australia Southeast";
                case "CentralIndia":
                    return "Central India";
                case "SouthIndia":
                    return "South India";
                case "WestIndia":
                    return "West India";
                case "CanadaCentral":
                    return "Canada Central";
                case "CanadaEast":
                    return "Canada East";
                case "UkSouth":
                    return "UK South";
                case "UkWest":
                    return "UK West";
                default:
                    return "Undefined";
            }
        }
    }
}
