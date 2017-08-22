using System;
using System.Data.SqlClient;
using Octopus.Diagnostics;
using Octopus.Shared.Configuration;

namespace Octopus.Shared.Startup
{
    public class DeleteInstanceCommand : AbstractCommand
    {
        readonly IApplicationInstanceSelector instanceSelector;
        readonly ILog log;
        string instanceName;

        public DeleteInstanceCommand(IApplicationInstanceSelector instanceSelector, ILog log)
        {
            this.instanceSelector = instanceSelector;
            this.log = log;
            Options.Add("instance=", "Name of the instance to delete", v => instanceName = v);
        }

        protected override void Start()
        {
            RemoveNodeFromDatabase();

            if (string.IsNullOrWhiteSpace(instanceName))
            {
                instanceSelector.DeleteDefaultInstance();
            }
            else
            {
                instanceSelector.DeleteInstance(instanceName);
            }
        }

        private void RemoveNodeFromDatabase()
        {
            var connectionString = instanceSelector.GetCurrentInstance().Configuration.Get<string>("Octopus.Storage.ExternalDatabaseConnectionString");
            var serverName = instanceSelector.GetCurrentInstance().Configuration.Get<string>("Octopus.Server.NodeName");

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(serverName))
                return;

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    const string commandText = "DELETE FROM OctopusServerNode WHERE [Id] = @serverName";
                    using (var cmd = new SqlCommand(commandText, connection))
                    {
                        cmd.Parameters.AddWithValue("serverName", serverName);
                        cmd.ExecuteNonQuery();
                    }
                    log.Info($"Deregistered {instanceName} from the database");
                }
                catch (Exception)
                {                    
                    log.Warn("Could not open the database. This instance has not been deregistered.");
                }
            }            
        }
    }
}