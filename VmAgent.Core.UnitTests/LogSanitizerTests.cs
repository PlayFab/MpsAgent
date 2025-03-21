using Microsoft.Azure.Gaming.VmAgent.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VmAgent.Core.UnitTests
{
    [TestClass]
    public class LogSanitizerTests
    {
        [TestMethod]
        [TestCategory("BVT")]
        [DataRow("downloadUri:myuri.com?sig=abcdefghijklnxyFz81trcLBsKdiAot9HRdiorp5kdnGv%2BI%3D", "downloadUri:myuri.com?[REDACTED]")]
        [DataRow("StartGameCommand : MyExecutable.exe -log -secretKey=ABCDE", "StartGameCommand : MyExecutable.exe -log -[REDACTED]")]
        public void Sanitize_ShouldRedactSensitiveData(string input, string expected)
        {
            // Act
            var result = LogSanitizer.Sanitize(input);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
