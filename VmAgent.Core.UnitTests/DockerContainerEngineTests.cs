using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Azure.Gaming.VmAgent.ContainerEngines;
using Microsoft.Azure.Gaming.AgentInterfaces;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class DockerContainerEngineTests
    {
        [TestInitialize]
        public void BeforeEachTest()
        {
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestGetImageNameWithTag()
        {
            var imageDetails = new ContainerImageDetails
            {
                ImageName = "name",
                ImageTag = "tag",
                ImageDigest = null,
                Registry = null,
            };
            Assert.AreEqual("name:tag", DockerContainerEngine.GetImageNameFromContainerImageDetails(imageDetails));

            imageDetails.Registry = "registry";
            Assert.AreEqual("registry/name:tag", DockerContainerEngine.GetImageNameFromContainerImageDetails(imageDetails));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestGetImageNameWithDigest()
        {
            var imageDetails = new ContainerImageDetails
            {
                ImageName = "name",
                ImageTag = "tag",
                ImageDigest = "digest",
                Registry = null,
            };
            Assert.AreEqual("name@digest", DockerContainerEngine.GetImageNameFromContainerImageDetails(imageDetails));

            imageDetails.Registry = "registry";
            Assert.AreEqual("registry/name@digest", DockerContainerEngine.GetImageNameFromContainerImageDetails(imageDetails));
        }
    }
}