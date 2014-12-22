using System;
using System.Reflection;
using Octopus.Shared.Configuration;
using Octopus.Shared.Diagnostics;
using Octopus.Shared.Security.Masking;
using Octopus.Shared.Util;

namespace Octopus.Shared.Startup
{
    public class ServiceCommand : AbstractStandardCommand
    {
        readonly ILog log;
        readonly string serviceDescription;
        readonly Assembly assemblyContainingService;
        readonly IApplicationInstanceSelector instanceSelector;
        readonly ServiceConfigurationState serviceConfigurationState;

        public ServiceCommand(IApplicationInstanceSelector instanceSelector, string serviceDescription, Assembly assemblyContainingService, ILog log) : base(instanceSelector)
        {
            this.instanceSelector = instanceSelector;
            this.serviceDescription = serviceDescription;
            this.assemblyContainingService = assemblyContainingService;
            this.log = log;

            serviceConfigurationState = new ServiceConfigurationState();

            Options.Add("start", "Start the Windows Service if it is not already running", v => serviceConfigurationState.Start = true);
            Options.Add("stop", "Stop the Windows Service if it is running", v => serviceConfigurationState.Stop = true);
            Options.Add("reconfigure", "Reconfigure the Windows Service", v => serviceConfigurationState.Reconfigure = true);
            Options.Add("install", "Install the Windows Service", v => serviceConfigurationState.Install = true);
            Options.Add("username=", "Username to run the service under (DOMAIN\\Username format). Only used when --install is used.", v => serviceConfigurationState.Username = v);
            Options.Add("uninstall", "Uninstall the Windows Service", v => serviceConfigurationState.Uninstall = true);
            Options.Add("password=", "Password for the username specified with --username. Only used when --install is used.", v =>
            {
                serviceConfigurationState.Password = v;
                MaskingContext.Permanent.MaskInstancesOf(v);
            });

        }
        protected override void Start()
        {
            base.Start();

            var thisServiceName = ServiceName.GetWindowsServiceName(instanceSelector.Current.ApplicationName, instanceSelector.Current.InstanceName);
            var instance = instanceSelector.Current.InstanceName;
            var exePath = assemblyContainingService.FullLocalPath();

            var serverInstaller = new ConfigureServiceHelper(log, thisServiceName, exePath, instance, serviceDescription, serviceConfigurationState);

            serverInstaller.ConfigureService();
        }

    }
}