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
            Assert.AreEqual("name:tag", DockerContainerEngine.GetImageNameAndTagFromContainerImageDetails(imageDetails));

            imageDetails.Registry = "registry";
            Assert.AreEqual("registry/name:tag", DockerContainerEngine.GetImageNameAndTagFromContainerImageDetails(imageDetails));
        }
    }
}