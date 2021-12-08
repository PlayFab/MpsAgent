namespace Microsoft.Azure.Gaming.VmAgent.Core.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using VisualStudio.TestTools.UnitTesting;

    public class ExceptionAssert
    {
        public static T Throws<T>(Action testAction)
            where T : Exception
        {
            Exception actualException = null;
            try
            {
                testAction();
            }
            catch (Exception ex)
            {
                actualException = ex;
            }

            if (actualException == null)
            {
                Assert.Fail($"No exception was thrown. Expected exception - {typeof(T).FullName}");
            }

            Assert.AreEqual(
                typeof(T).FullName,
                actualException.GetType().FullName,
                "Unexpected exception type: {0}",
                actualException);

            return (T)actualException;
        }
    }
}
