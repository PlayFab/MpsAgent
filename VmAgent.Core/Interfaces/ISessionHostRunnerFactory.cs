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
        /// <param name="sessionHostType">The type of session host / game server to run on the VM.</param>
        /// <param name="vmConfiguration">The Vm configuration.</param>
        /// <param name="logger">A logger instance.</param>
        /// <param name="shouldPublicPortMatchGamePort">Whether the port number that clients connect to, on the load balancer, should match what server listens to, on the VM.</param>
        /// <returns></returns>
        ISessionHostRunner CreateSessionHostRunner(SessionHostType sessionHostType, VmConfiguration vmConfiguration, MultiLogger logger, bool shouldPublicPortMatchGamePort);

        /// <summary>
        /// Creates an <see cref="ISessionHostRunner"/> based on the specified <paramref name="sessionHostType"/>,
        /// </summary>
        /// <param name="sessionHostType">The type of session host / game server to run on the VM.</param>
        /// <param name="vmConfiguration">The Vm configuration.</param>
        /// <param name="logger">A logger instance.</param>
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

        /// <inheritdoc />
        public ISessionHostRunner CreateSessionHostRunner(SessionHostType sessionHostType, VmConfiguration vmConfiguration, MultiLogger logger, bool shouldPublicPortMatchGamePort)
        {
            return sessionHostType == SessionHostType.Container
                ? (ISessionHostRunner)new DockerContainerEngine(vmConfiguration, logger, SystemOperations.Default, shouldPublicPortMatchGamePort: shouldPublicPortMatchGamePort)
                : new ProcessRunner(vmConfiguration, logger, SystemOperations.Default, new ProcessWrapper());
        }

        /// <inheritdoc />
        public ISessionHostRunner CreateSessionHostRunner(SessionHostType sessionHostType, VmConfiguration vmConfiguration, MultiLogger logger)
        {
            return CreateSessionHostRunner(sessionHostType, vmConfiguration, logger, false);
        }
    }
}
