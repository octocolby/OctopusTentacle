﻿using System;
using Octopus.Shared.Variables;
using Pipefish;

namespace Octopus.Shared.Messages.Deploy.Steps
{
    public class OutputVariablesCollectedEvent : IMessage
    {
        public string MachineId { get; private set; }
        public string DeploymentActionId { get; private set; }
        public VariableDictionary Variables { get; private set; }

        public OutputVariablesCollectedEvent(string deploymentActionId, VariableDictionary variables, string machineId = null)
        {
            MachineId = machineId;
            DeploymentActionId = deploymentActionId;
            Variables = variables;
        }
    }
}
