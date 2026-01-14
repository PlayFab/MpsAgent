using Microsoft.Azure.Gaming.AgentInterfaces;
using Microsoft.Azure.Gaming.VmAgent.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class VmPersistedStateTests
    {
        [TestMethod]
        [TestCategory("BVT")]
        public void ToRedactedString()
        {
            var persistedState = new VmPersistedState()
            {
                AssetRetrievalResult = ResourceRetrievalResult.ResourceNotAvailable,
                GameResourceDetails = new SessionHostsStartInfo()
                {
                    AssetDetails = [
                        new AssetDetail()
                        {
                            LocalFilePath = @"C:\Assets\myAsset.zip",
                            SasTokens = ["SECRET", "ANOTHER_SECRET"],
                            DownloadUris = ["https://storage.azure.com/file1", "https://storage.azure.com/file2" ]
                        },
                        new AssetDetail()
                        {
                            LocalFilePath = @"C:\Assets\myAsset2.zip",
                            SasTokens = ["SECRET", "ANOTHER_SECRET"],
                            DownloadUris = ["https://storage.azure.com/file3", "https://storage.azure.com/file4" ]
                        }
                    ],
                    AssignmentId = "id",
                    Count = 2,
                    DeploymentMetadata = new Dictionary<string, string>()
                    {
                        { "version", "0.1" },
                        { "createdDate", "yesterday" }
                    },
                    GameCertificates = [
                        new CertificateDetail() {
                            Name = "cert1",
                            PemContents = "BASE64_SECRET",
                        },
                        new CertificateDetail() {
                            Name = "cert2",
                            PfxContents = new byte[2],
                        }
                    ],
                    GameSecrets = [
                        new SecretDetail() {
                            Name = "cert1",
                            Value = "BASE64_SECRET",
                        },
                        new SecretDetail() {
                            Name = "cert2",
                            Value = "SECRET",
                        }
                    ],
                    ImageDetails = new ContainerImageDetails()
                    {
                        ImageName = "myImage",
                        Username = "SECRET",
                        Password = "SECRET",
                    },
                    LogUploadParameters = new LogUploadParameters()
                    {
                        BlobServiceEndpoint = "hi.com",
                        SharedAccessSignatureToken = "SECRET_TOKEN",
                    }
                },
                VmState = VmState.Assigned,
            };

            string originalString = JsonConvert.SerializeObject(persistedState);
            Assert.IsTrue(originalString.Contains("SECRET"));

            string redactedString = persistedState.ToRedactedString();
            Assert.IsFalse(redactedString.Contains("SECRET"));
            Assert.IsTrue(redactedString.Contains("hi.com")); // sanity check
            Assert.IsTrue(redactedString.Contains("https://storage.azure.com/file1")); // sanity check
        }
    }
}
