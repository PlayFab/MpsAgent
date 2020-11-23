// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The type of metrics contained in the PerformanceMetrics object. 
    /// See the PerformanceMetrics object for more information about how 
    /// these two types differ.
    /// </summary>
    public enum PerformanceMetricsType
    {
        Machine,
        Process
    }

    /// <summary>
    /// PerformanceMetricsCounterLevels can be used to control what information is included
    /// in the PerformanceMetrics object or what PerformanceMetrics objects are created. 
    /// </summary>
    [Flags]
    public enum PerformanceMetricsCounterLevel
    {
        MachineCounters = 0x1,
        ProcessCounters = 0x2,
        CpuCounters = 0x4,
        DiskCounters = 0x8,
        MemoryCounters = 0x10,
        NetworkCounters = 0x20,

        Default = All,
        All = MachineCounters | ProcessCounters | CpuCounters | MemoryCounters | DiskCounters | NetworkCounters,
        AllMachineCounters = MachineCounters | CpuCounters | MemoryCounters | DiskCounters | NetworkCounters,
        AllProcessCounters = ProcessCounters | CpuCounters | MemoryCounters | DiskCounters | NetworkCounters,
        AllCpuCounters = MachineCounters | ProcessCounters | CpuCounters,
        AllMemoryCounters = MachineCounters | ProcessCounters | MemoryCounters,
        AllDiskCounters = MachineCounters | ProcessCounters | DiskCounters,
        AllNetworkCounters = MachineCounters | ProcessCounters | NetworkCounters
    }

    /// <summary>
    /// String versions of PerformanceMetrics properties.
    /// </summary>
    public static class PerformanceMetricsProperties
    {
        public const string MemoryAvailable = "MemoryAvailable";
        public const string MemoryUsed = "MemoryUsed";
        public const string CpuPercentProcessorTimePerProcessor = "CpuPercentProcessorTimePerProcessor";
        public const string CpuPercentUserTimePerProcessor = "CpuPercentUserTimePerProcessor";
        public const string DiskReadOperationsPerSecond = "DiskReadOperationsPerSecond";
        public const string DiskReadBytesPerSecond = "DiskReadBytesPerSecond";
        public const string DiskWriteOperationsPerSecond = "DiskWriteOperationsPerSecond";
        public const string DiskWriteBytesPerSecond = "DiskWriteBytesPerSecond";

        public const string IPv4DatagramsReceivedPerSecond = "IPv4DatagramsReceivedPerSecond";
        public const string IPv4DatagramsSentPerSecond = "IPv4DatagramsSentPerSecond";
        public const string IPv6DatagramsReceivedPerSecond = "IPv6DatagramsReceivedPerSecond";
        public const string IPv6DatagramsSentPerSecond = "IPv6DatagramsSentPerSecond";
        public const string NetworkInterfaceBytesReceivedPerSecond = "NetworkInterfaceBytesReceivedPerSecond";
        public const string NetworkInterfaceBytesSentPerSecond = "NetworkInterfaceBytesSentPerSecond";
        public const string NetworkInterfacePacketsReceivedPerSecond = "NetworkInterfacePacketsReceivedPerSecond";
        public const string NetworkInterfacePacketsSentPerSecond = "NetworkInterfacePacketsSentPerSecond";
        public const string NetworkInterfaceOutputQueueLength = "NetworkInterfaceOutputQueueLength";
        public const string NetworkAdapterBytesReceivedPerSecond = "NetworkAdapterBytesReceivedPerSecond";
        public const string NetworkAdapterBytesSentPerSecond = "NetworkAdapterBytesSentPerSecond";
        public const string NetworkAdapterPacketsReceivedPerSecond = "NetworkAdapterPacketsReceivedPerSecond";
        public const string NetworkAdapterPacketsSentPerSecond = "NetworkAdapterPacketsSentPerSecond";
        public const string NetworkAdapterOutputQueueLength = "NetworkAdapterOutputQueueLength";
        public const string InterruptsPerSecondPerProcessor = "InterruptsPerSecondPerProcessor";
        public const string DPCsQueuedPerSecondPerProcessor = "DPCsQueuedPerSecondPerProcessor";

        public static string[] GetPropertiesList()
        {
            return new[] {
                MemoryAvailable,
                MemoryUsed,
                CpuPercentProcessorTimePerProcessor,
                CpuPercentUserTimePerProcessor,
                DiskReadOperationsPerSecond,
                DiskReadBytesPerSecond,
                DiskWriteOperationsPerSecond,
                DiskWriteBytesPerSecond,
                IPv4DatagramsReceivedPerSecond,
                IPv4DatagramsSentPerSecond,
                IPv6DatagramsReceivedPerSecond,
                IPv6DatagramsSentPerSecond,
                NetworkInterfaceBytesReceivedPerSecond,
                NetworkInterfaceBytesSentPerSecond,
                NetworkInterfacePacketsReceivedPerSecond,
                NetworkInterfacePacketsSentPerSecond,
                NetworkInterfaceOutputQueueLength,
                NetworkAdapterBytesReceivedPerSecond,
                NetworkAdapterBytesSentPerSecond,
                NetworkAdapterPacketsReceivedPerSecond,
                NetworkAdapterPacketsSentPerSecond,
                NetworkAdapterOutputQueueLength,
                InterruptsPerSecondPerProcessor,
                DPCsQueuedPerSecondPerProcessor
            };
        }
    }

    /// <summary>
    /// The PerformanceMetrics class is used a contract between collectors and reporters
    /// for all OS types. 
    /// The below properties that reference a Windows Performance Counter can be tested in
    /// powershell by running this command with the counter path listed:
    ///  PS Get-Counter -Counter "{counter-path}"
    ///  Example: PS Get-Counter -Counter "\Processor(*)\DPCs Queued/sec"
    /// </summary>
    public class PerformanceMetrics
    {
        /// <summary>
        /// MachineLevelName should be used to fill the Name property of a 
        /// machine level performance metric.
        /// </summary>
        public static readonly string MachineLevelName = "Machine";

        /// <summary>
        /// CounterTotal should be used as the key for performance metrics
        /// that represent the total value from a counter.
        /// </summary>
        public static readonly string CounterTotalKey = "_Total";

        /// <summary>
        /// Type of PerformanceMetric:
        ///     Machine Level = PerformanceMetricsType.Machine
        ///     Process Level = PerformanceMetricsType.Process
        /// </summary>
        public PerformanceMetricsType Type { get; set; }

        /// <summary>
        /// Name of the PerformanceMetric:
        ///     Machine Level = MachineLevelName (string above)
        ///     Process Level = processName#instanceNum ex: chrome#2
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the memory that
        /// is currently available on the machine. This is only valid for 
        /// machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Memory\Available MBytes = Amount of physical memory available MB
        /// </summary>
        public Dictionary<string, float> MemoryAvailable { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the memory that
        /// is currently being used by a process. This is only valid for 
        /// process level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Process(*)\Working Set = The current private + shared memory for this process, in bytes.
        /// </summary>
        public Dictionary<string, float> MemoryUsed { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the current cpu 
        /// usage per logical processor. 
        /// On Windows, it refers to the these counters: 
        ///     Windows Machine Level = \Processor\% Processor Time = Shows the percentage of elapsed time that this thread used the processor to execute instructions.
        ///     Windows Process Level = \Process(*)\% Processor Time = Shows the percentage of time that the processor spent executing a non-idle thread.
        /// </summary>
        public Dictionary<string, float> CpuPercentProcessorTimePerProcessor { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the current cpu 
        /// usage per logical processor in user modes. 
        /// On Windows, it refers to the these counters: 
        ///     Windows Machine Level = \Processor(*)\% User Time = Shows the percentage of elapsed time that this thread spent executing code in user mode. 
        ///     Windows Process Level = \Process(*)\% User Time = Shows the percentage of time that the processor spent executing code in user mode. 
        /// </summary>
        public Dictionary<string, float> CpuPercentUserTimePerProcessor { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for Disk read operations per second.
        /// On Windows, it refers to the these counters: 
        ///     Windows Machine Level = \PhysicalDisk(*)\Disk Reads/sec = Shows the rate, in incidents per second, at which read operations were performed on the disk.
        ///     Windows Process Level = \Process(*)\IO Read Operations/sec = Shows the rate, in incidents per second, at which the process was issuing read I/O operations.
        /// </summary>
        public Dictionary<string, float> DiskReadOperationsPerSecond { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for Disk read bytes per second.
        /// On Windows, it refers to the these counters: 
        ///     Windows Machine Level = \PhysicalDisk(*)\Disk Read Bytes/sec = Shows the rate, in incidents per second, at which bytes were transferred from the disk during read operations.
        ///     Windows Process Level = \Process(*)\IO Read Bytes/sec = Shows the rate, in incidents per second, at which the process was reading bytes from I/O operations.
        /// </summary>
        public Dictionary<string, float> DiskReadBytesPerSecond { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for Disk write operations per second.
        /// On Windows, it refers to the these counters: 
        ///     Windows Machine Level = \PhysicalDisk(*)\Disk Writes/sec = Shows the rate, in incidents per second, at which write operations were performed on the disk.
        ///     Windows Process Level = \Process(*)\IO Write Operations/sec = Shows the rate, in incidents per second, at which the process was issuing write I/O operations.
        /// </summary>
        public Dictionary<string, float> DiskWriteOperationsPerSecond { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for Disk write bytes per second.
        /// On Windows, it refers to the these counters: 
        ///     Windows Machine Level = \PhysicalDisk(*)\Disk Write Bytes/sec = Shows the rate, in incidents per second, at which bytes were transferred to the disk during write operations.
        ///     Windows Process Level = \Process(*)\IO Write Bytes/sec = Shows the rate, in incidents per second, at which the process was writing bytes to I/O operations.
        /// </summary>
        public Dictionary<string, float> DiskWriteBytesPerSecond { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the number of datagrams received per second.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \IPv4\Datagrams Received/sec
        /// </summary>
        public Dictionary<string, float> IPv4DatagramsReceivedPerSecond { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the number of datagrams sent per second.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \IPv4\Datagrams Sent/sec
        /// </summary>
        public Dictionary<string, float> IPv4DatagramsSentPerSecond { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the number of datagrams received per second.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \IPv6\Datagrams Received/sec
        /// </summary>
        public Dictionary<string, float> IPv6DatagramsReceivedPerSecond { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the number of datagrams sent per second.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \IPv6\Datagrams Received/sec
        /// </summary>
        public Dictionary<string, float> IPv6DatagramsSentPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the number of bytes received per second
        /// for each network interface.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Interface(*)\Bytes Received/sec
        /// </summary>
        public Dictionary<string, float> NetworkInterfaceBytesReceivedPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the number of bytes sent per second
        /// for each network interface.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Interface(*)\Bytes Sent/sec
        /// </summary>
        public Dictionary<string, float> NetworkInterfaceBytesSentPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the number of Packets received per second
        /// for each network interface.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Interface(*)\Packets Received/sec
        /// </summary>
        public Dictionary<string, float> NetworkInterfacePacketsReceivedPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the number of Packets sent per second
        /// for each network interface.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Interface(*)\Packets Sent/sec
        /// </summary>
        public Dictionary<string, float> NetworkInterfacePacketsSentPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the Output Queue Length  (in packets)
        /// for each network interface. If this is longer than 2, delays occur. Value should be zero.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Interface(*)\Output Queue Length
        /// </summary>
        public Dictionary<string, float> NetworkInterfaceOutputQueueLength { get; set; }

        /// <summary>
        /// Map of instances for the number of bytes received per second
        /// for each network adapter.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Adapter(*)\Bytes Received/sec
        /// </summary>
        public Dictionary<string, float> NetworkAdapterBytesReceivedPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the number of bytes sent per second
        /// for each network adapter.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Adapter(*)\Bytes Sent/sec
        /// </summary>
        public Dictionary<string, float> NetworkAdapterBytesSentPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the number of Packets received per second
        /// for each network adapter.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Adapter(*)\Packets Received/sec
        /// </summary>
        public Dictionary<string, float> NetworkAdapterPacketsReceivedPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the number of Packets sent per second
        /// for each network adapter.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Adapter(*)\Packets Sent/sec
        /// </summary>
        public Dictionary<string, float> NetworkAdapterPacketsSentPerSecond { get; set; }

        /// <summary>
        /// Map of instances for the Output Queue Length  (in packets)
        /// for each network adapter. If this is longer than 2, delays occur. Value should be zero.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows it refers to the this counter: 
        ///     \Network Adapter(*)\Output Queue Length
        /// </summary>
        public Dictionary<string, float> NetworkAdapterOutputQueueLength { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the number of interrupts
        /// per second per processor.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows, it refers to the these counters: 
        ///     \Processor(*)\Interrupts/sec
        /// </summary>
        public Dictionary<string, float> InterruptsPerSecondPerProcessor { get; set; }

        /// <summary>
        /// Map of instances (including CounterTotal) for the number of DPCs queued
        /// per second per processor.
        /// This is only valid for machine level PerformanceMetrics. 
        /// On Windows, it refers to the these counters: 
        ///     \Processor(*)\DPCs Queued/sec
        /// </summary>
        public Dictionary<string, float> DPCsQueuedPerSecondPerProcessor { get; set; }

        /// <summary>
        /// Use this method to set the value of each property by string.
        /// </summary>
        /// <param name="propertyName">Name of the property as a string.</param>
        /// <param name="propertyValue">The value for the property to be set.</param>
        public void SetProperty(string propertyName, Dictionary<string, float> propertyValue)
        {
            if (propertyName == null)
            {
                throw new MissingFieldException(
                    "'null' is not a valid property of a PerformanceMetrics object.");
            }

            if (propertyName == PerformanceMetricsProperties.MemoryAvailable)
            {
                MemoryAvailable = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.MemoryUsed)
            {
                MemoryUsed = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.CpuPercentProcessorTimePerProcessor)
            {
                CpuPercentProcessorTimePerProcessor = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.CpuPercentUserTimePerProcessor)
            {
                CpuPercentUserTimePerProcessor = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskReadOperationsPerSecond)
            {
                DiskReadOperationsPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskReadBytesPerSecond)
            {
                DiskReadBytesPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskWriteOperationsPerSecond)
            {
                DiskWriteOperationsPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskWriteBytesPerSecond)
            {
                DiskWriteBytesPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv4DatagramsReceivedPerSecond)
            {
                IPv4DatagramsReceivedPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv4DatagramsSentPerSecond)
            {
                IPv4DatagramsSentPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv6DatagramsReceivedPerSecond)
            {
                IPv6DatagramsReceivedPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv6DatagramsSentPerSecond)
            {
                IPv6DatagramsSentPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfaceBytesReceivedPerSecond)
            {
                NetworkInterfaceBytesReceivedPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfaceBytesSentPerSecond)
            {
                NetworkInterfaceBytesSentPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfacePacketsReceivedPerSecond)
            {
                NetworkInterfacePacketsReceivedPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfacePacketsSentPerSecond)
            {
                NetworkInterfacePacketsSentPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfaceOutputQueueLength)
            {
                NetworkInterfaceOutputQueueLength = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterBytesReceivedPerSecond)
            {
                NetworkAdapterBytesReceivedPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterBytesSentPerSecond)
            {
                NetworkAdapterBytesSentPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterPacketsReceivedPerSecond)
            {
                NetworkAdapterPacketsReceivedPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterPacketsSentPerSecond)
            {
                NetworkAdapterPacketsSentPerSecond = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterOutputQueueLength)
            {
                NetworkAdapterOutputQueueLength = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.InterruptsPerSecondPerProcessor)
            {
                InterruptsPerSecondPerProcessor = propertyValue;
            }
            else if (propertyName == PerformanceMetricsProperties.DPCsQueuedPerSecondPerProcessor)
            {
                DPCsQueuedPerSecondPerProcessor = propertyValue;
            }
            else
            {
                throw new MissingFieldException(
                    $"{propertyName} cannot be found as a property of a PerformanceMetrics object.");
            }
        }

        /// <summary>
        /// Use this method to set the value of each property by string.
        /// </summary>
        /// <param name="propertyName">Name of the property as a string.</param>
        public Dictionary<string, float> GetProperty(string propertyName)
        {
            if (propertyName == null)
            {
                throw new MissingFieldException(
                    "'null' is not a valid property of a PerformanceMetrics object.");
            }

            if (propertyName == PerformanceMetricsProperties.MemoryAvailable)
            {
                return MemoryAvailable;
            }
            else if (propertyName == PerformanceMetricsProperties.MemoryUsed)
            {
                return MemoryUsed;
            }
            else if (propertyName == PerformanceMetricsProperties.CpuPercentProcessorTimePerProcessor)
            {
                return CpuPercentProcessorTimePerProcessor;
            }
            else if (propertyName == PerformanceMetricsProperties.CpuPercentUserTimePerProcessor)
            {
                return CpuPercentUserTimePerProcessor;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskReadOperationsPerSecond)
            {
                return DiskReadOperationsPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskReadBytesPerSecond)
            {
                return DiskReadBytesPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskWriteOperationsPerSecond)
            {
                return DiskWriteOperationsPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.DiskWriteBytesPerSecond)
            {
                return DiskWriteBytesPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv4DatagramsReceivedPerSecond)
            {
                return IPv4DatagramsReceivedPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv4DatagramsSentPerSecond)
            {
                return IPv4DatagramsSentPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv6DatagramsReceivedPerSecond)
            {
                return IPv6DatagramsReceivedPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.IPv6DatagramsSentPerSecond)
            {
                return IPv6DatagramsSentPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfaceBytesReceivedPerSecond)
            {
                return NetworkInterfaceBytesReceivedPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfaceBytesSentPerSecond)
            {
                return NetworkInterfaceBytesSentPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfacePacketsReceivedPerSecond)
            {
                return NetworkInterfacePacketsReceivedPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfacePacketsSentPerSecond)
            {
                return NetworkInterfacePacketsSentPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkInterfaceOutputQueueLength)
            {
                return NetworkInterfaceOutputQueueLength;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterBytesReceivedPerSecond)
            {
                return NetworkAdapterBytesReceivedPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterBytesSentPerSecond)
            {
                return NetworkAdapterBytesSentPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterPacketsReceivedPerSecond)
            {
                return NetworkAdapterPacketsReceivedPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterPacketsSentPerSecond)
            {
                return NetworkAdapterPacketsSentPerSecond;
            }
            else if (propertyName == PerformanceMetricsProperties.NetworkAdapterOutputQueueLength)
            {
                return NetworkAdapterOutputQueueLength;
            }
            else if (propertyName == PerformanceMetricsProperties.InterruptsPerSecondPerProcessor)
            {
                return InterruptsPerSecondPerProcessor;
            }
            else if (propertyName == PerformanceMetricsProperties.DPCsQueuedPerSecondPerProcessor)
            {
                return DPCsQueuedPerSecondPerProcessor;
            }
            else
            {
                throw new MissingFieldException(
                    $"{propertyName} cannot be found as a property of a PerformanceMetrics object.");
            }
        }
    }
}
