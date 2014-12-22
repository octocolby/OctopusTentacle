using System;
using System.Collections.Generic;
using Pipefish;

namespace Octopus.Shared.Messages.Deploy.Acquire
{
    public class PackagesAcquiredEvent : IMessage
    {
        public List<AcquiredPackage> AcquiredPackages { get; private set; }

        public PackagesAcquiredEvent(List<AcquiredPackage> acquiredPackages)
        {
            AcquiredPackages = acquiredPackages;
        }
    }
}