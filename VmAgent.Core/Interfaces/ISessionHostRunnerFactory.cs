// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using AgentInterfaces;
    using ContainerEngines;

    /// <summary>
    /// A factory that's used to create an <see cref="ISessionHostRunner"/> depending on <see cref="SessionHostType"/>.
    /// </summary>
    public interface ISessionHostRunnerFactory
    {
        /// <summary>
        /// Creates an <see cref="ISessionHostRunner"/> based on the specified <paramref name="sessionHostType"/>,
        /// </summary>
        /// <param name="sessionHostType"></param>
        /// <param name="vmConfiguration"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        ISessionHostRunner CreateSessionHostRunner(SessionHostType sessionHostType, VmConfiguration vmConfiguration, MultiLogger logger);
    }

    /// <summary>
    /// A factory that's used to create an <see cref="ISessionHostRunner"/> depending on <see cref="SessionHostType"/>.
    /// </summary>
    public class SessionHostRunnerFactory : ISessionHostRunnerFactory
    {
        public static readonly SessionHostRunnerFactory Instance = new SessionHostRunnerFactory();

        private SessionHostRunnerFactory()
        {
        }

        //<inheritdoc/>
        public ISessionHostRunner CreateSessionHostRunner(SessionHostType sessionHostType, VmConfiguration vmConfiguration, MultiLogger logger)
        {
            return sessionHostType == SessionHostType.Container
                ? (ISessionHostRunner)new DockerContainerEngine(vmConfiguration, logger, SystemOperations.Default)
                : new ProcessRunner(vmConfiguration, logger, SystemOperations.Default, new ProcessWrapper());
        }
    }
}
