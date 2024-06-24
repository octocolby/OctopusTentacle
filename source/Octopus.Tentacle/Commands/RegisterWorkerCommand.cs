﻿using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Client.Operations;
using Octopus.Diagnostics;
using Octopus.Tentacle.Commands.OptionSets;
using Octopus.Tentacle.Communications;
using Octopus.Tentacle.Configuration;
using Octopus.Tentacle.Configuration.Instances;
using Octopus.Tentacle.Startup;

namespace Octopus.Tentacle.Commands
{
    public class RegisterWorkerCommand : RegisterMachineCommandBase<IRegisterWorkerOperation>
    {
        readonly List<string> workerpools = new List<string>();

        public RegisterWorkerCommand(Lazy<IRegisterWorkerOperation> lazyRegisterMachineOperation,
            Lazy<IWritableTentacleConfiguration> configuration,
            ISystemLog log,
            IApplicationInstanceSelector selector,
            Lazy<IOctopusServerChecker> octopusServerChecker,
            IProxyConfigParser proxyConfig,
            IOctopusClientInitializer octopusClientInitializer,
            ISpaceRepositoryFactory spaceRepositoryFactory,
            ILogFileOnlyLogger logFileOnlyLogger)
            : base(lazyRegisterMachineOperation, configuration, log, selector, octopusServerChecker, proxyConfig, octopusClientInitializer, spaceRepositoryFactory, logFileOnlyLogger)
        {
            Options.Add("workerpool=", "The worker pool name, slug or Id to add the machine to - e.g., 'Windows Pool'; specify this argument multiple times to add to multiple pools", s => workerpools.Add(s));
        }

        protected override void CheckArgs()
        {
            if (workerpools.Count == 0 || string.IsNullOrWhiteSpace(workerpools.First()))
                throw new ControlledFailureException("Please specify a worker pool name, slug or Id, e.g., --workerpool=Default");
        }

        protected override void EnhanceOperation(IRegisterWorkerOperation registerOperation)
        {
            registerOperation.WorkerPools = workerpools.ToArray();
        }
    }
}