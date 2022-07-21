// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    public static class MetricConstants
    {
        public const string WaitForAssetReplication = "WaitForAssetReplication";
        public const string DownloadAsset = "DownloadAsset";
        public const string ExtractAndCopyAssets = "ExtractAndCopyAssets";
        public const string PullImage = "PullImage";
        public const string PushImage = "PushImage";
        public const string BlobUri = "BlobUri";
        public const string DownloadDurationInMilliseconds = "DownloadDuration";
        public const string ExtractDurationInMilliseconds = "ExtractDuration";
        public const string CompressDurationInMilliseconds = "CompressDuration";
        public const string DeleteDurationInMilliseconds = "DeleteDuration";
        public const string UploadDurationInMilliseconds = "UploadDuration";
        public const string CopyDurationInMilliseconds = "CopyDuration";
        public const string WaitDurationInMilliseconds = "WaitDuration";
        public const string SizeInBytes = "Size";
        public const string BytesCopied = "BytesCopied";
        public const string BytesAlreadyCopied = "BytesAlreadyCopied";
        public const string TitleId = "TitleId";
        public const string DeploymentId = "DeploymentId";
        public const string Region = "Region";
        public const string UpdatedPersistedState = "UpdatedPersistedState";
        public const string UploadedLogs = "UploadedLogs";
        public const string Attempt = "Attempt";
        public const string PersistStateDurationMs = "PersistStateDurationMs";
        public const string StartFileWrite = "StartFileWrite";
        public const string TimeToStartFileWriteMs = "TimeToStartFileWriteMs";
        public const string ContainerStats = "ContainerStats";
        public const string ContainerCreationTime = "ContainerCreationTimeMs";
        public const string ContainerStartTime = "ContainerStartTimeMs";
        public const string MaintenanceScheduleRequests = "MaintenanceScheduleRequests";
        public const string AzureFabricVmNameRequest = "AzureFabricVmNameRequest";
        public const string PeriodicGetScheduleRequest = "PeriodicGetScheduleRequest";
        public const string BlobLastModifiedTime = "LastModifiedTime";
        public const string FileWriteDurationMs = "FileWriteDurationMs";
        public const string MdmInstall = "MdmInstall";
        public const string MdmStart = "MdmStart";
        public const string TimeToEnterLockMs = "TimeToEnterLockMs";
        public const string StateType = "StateType";
        public const string CallerName = "CallerName";
    }
}
