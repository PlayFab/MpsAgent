// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.LocalMultiplayerAgent.Config
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using AgentInterfaces;
    using VmAgent.Core.Interfaces;

    public class MultiplayerSettingsValidator
    {
        private readonly ISystemOperations _systemOperations;
        private readonly MultiplayerSettings _settings;
        
        public MultiplayerSettingsValidator(MultiplayerSettings settings, ISystemOperations systemOperations = null)
        {
            if (settings == null)
            {
                throw new ArgumentException("Settings cannot be null");
            }
            _settings = settings;

            _systemOperations = systemOperations ?? SystemOperations.Default;
        }
    
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(_settings.OutputFolder) || string.IsNullOrWhiteSpace(_settings.TitleId))
            {
                throw new Exception("OutputFolder or TitleId not found. Call SetDefaultsIfNotSpecified() before this method");
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Running LocalMultiplayerAgent on Linux is not supported yet.");
                return false;
            }

            if (Globals.GameServerEnvironment == GameServerEnvironment.Linux && !_settings.RunContainer)
            {
                Console.WriteLine("The specified settings are invalid. Using Linux Game Servers requires running in a container.");
                return false;
            }

            string startGameCommand;

            if (_settings.RunContainer)
            {
                if (_settings.ContainerStartParameters == null)
                {
                    Console.WriteLine("No ContainerStartParameters were specified (and RunContainer is true).");
                    return false;
                }
                startGameCommand = _settings.ContainerStartParameters.StartGameCommand;
            }
            else
            {
                if (_settings.ProcessStartParameters == null)
                {
                    Console.WriteLine("No ProcessStartParameters were specified (and RunContainer is false).");
                    return false;
                }
                startGameCommand = _settings.ProcessStartParameters.StartGameCommand;
            }

            bool isSuccess = AreAssetsValid(_settings.AssetDetails);

            // StartGameCommand is optional on Linux
            if (string.IsNullOrWhiteSpace(startGameCommand))
            {
                if (Globals.GameServerEnvironment == GameServerEnvironment.Windows)
                {
                    Console.WriteLine("StartGameCommand must be specified.");
                    isSuccess = false;
                }
            }
            else
            {
                if (startGameCommand.Contains("<your_game_server_exe>"))
                {
                    Console.WriteLine($"StartGameCommand '{startGameCommand}' is invalid");
                    isSuccess = false;
                }
            }

            if (_settings.GameCertificateDetails?.Length > 0)
            {
                HashSet<string> names = new HashSet<string>();
                HashSet<string> paths = new HashSet<string>();

                foreach (GameCertificateDetails certDetails in _settings.GameCertificateDetails)
                {
                    if (string.IsNullOrEmpty(certDetails.Name.Trim()))
                    {
                        Console.WriteLine($"Certificate cannot have an empty name");
                        isSuccess = false;
                        continue;
                    }
                    if (string.IsNullOrEmpty(certDetails.Path) || !certDetails.Path.EndsWith(".pfx"))
                    {
                        Console.WriteLine($"Certificate with filename path '{certDetails.Path}' is not a pfx file");
                        isSuccess = false;
                        continue;
                    }
                    if (!_systemOperations.FileExists(certDetails.Path))
                    {
                        Console.WriteLine($"Certificate with filename {certDetails.Path} does not exist");
                        isSuccess = false;
                        continue;
                    }
                    if (!names.Add(certDetails.Name))
                    {
                        isSuccess = false;
                        Console.WriteLine($"Certificate with name {certDetails.Name} is included more than once");
                    }
                    if (!paths.Add(certDetails.Path))
                    {
                        isSuccess = false;
                        Console.WriteLine($"Certificate with path {certDetails.Path} is included more than once");
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(_settings.Region))
            {
                Console.WriteLine("Region must be specified.");
                isSuccess = false;
            }

            if (_settings.AgentListeningPort == 0)
            {
                Console.WriteLine("AgentListeningPort must be specified.");
                isSuccess = false;
            }

            if (!ulong.TryParse(_settings.TitleId, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out _))
            {
                Console.WriteLine("TitleId must be specified and be a valid hex number");
                isSuccess = false;
            }

            if (_settings.BuildId == Guid.Empty)
            {
                Console.WriteLine("BuildId must be specified.");
                isSuccess = false;
            }

            if (string.IsNullOrEmpty(_settings.SessionConfig?.SessionCookie))
            {
                Console.WriteLine("Warning: SessionCookie is not specified.");
            }

            if (_settings.AgentListeningPort != 56001)
            {
                Console.WriteLine($"Warning: You have specified an AgentListeningPort ({_settings.AgentListeningPort}) that is not the default.  Please make sure that port is open on your firewall by running setup.ps1 with the agent port specified.");
            }

            if (_settings.RunContainer) {
                if (_settings.NodePort == 0)
                {
                    Console.WriteLine("No NodePort was specified (and RunContainer is true).");
                    isSuccess = false;
                }
            }

            return isSuccess;

        }

        private bool AreAssetsValid(AssetDetail[] assetDetails)
        {
            if (assetDetails?.Length > 0)
            {
                foreach (AssetDetail detail in assetDetails)
                {
                    if (string.IsNullOrEmpty(detail.LocalFilePath))
                    {
                        Console.WriteLine("Asset details must contain local file path for each asset.");
                        return false;
                    }

                    if (_settings.RunContainer && string.IsNullOrEmpty(detail.MountPath))
                    {
                        Console.WriteLine("Asset details must contain mount path when running as container.");
                        return false;
                    }

                    if (!_systemOperations.FileExists(detail.LocalFilePath))
                    {
                        Console.WriteLine($"Asset {detail.LocalFilePath} was not found. Please specify path to a local zip file.");
                        return false;
                    }
                }

                return true;
            }

            if (Globals.GameServerEnvironment == GameServerEnvironment.Linux)
            {
                return true; // Assets are optional in Linux, since we're packing the entire game onto a container image
            }

            Console.WriteLine("Assets must be specified for game servers running on Windows.");
            return false;

        }
    }
}
