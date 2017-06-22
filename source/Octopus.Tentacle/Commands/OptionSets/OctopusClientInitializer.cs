﻿using System;
using System.Net;
using System.Threading.Tasks;
using Octopus.Client;
using Octopus.Client.Model;

namespace Octopus.Tentacle.Commands.OptionSets
{
    public class OctopusClientInitializer : IOctopusClientInitializer
    {
        public async Task<IOctopusAsyncClient> CreateClient(ApiEndpointOptions apiEndpointOptions, IWebProxy overrideProxy)
        {
            IOctopusAsyncClient client = null;
            try
            {
                if (string.IsNullOrWhiteSpace(apiEndpointOptions.ApiKey))
                {
                    var endpoint = new OctopusServerEndpoint(apiEndpointOptions.Server);
                    if (overrideProxy != null)
                    {
                        endpoint.Proxy = overrideProxy;
                    }
                    client = await OctopusAsyncClient.Create(endpoint).ConfigureAwait(false);
                    await client.Repository.Users
                        .SignIn(new LoginCommand { Username = apiEndpointOptions.Username, Password = apiEndpointOptions.Password });
                }
                else
                {
                    var endpoint = new OctopusServerEndpoint(apiEndpointOptions.Server, apiEndpointOptions.ApiKey, credentials: null);
                    if (overrideProxy != null)
                    {
                        endpoint.Proxy = overrideProxy;
                    }
                    client = await OctopusAsyncClient.Create(endpoint).ConfigureAwait(false);
                }
                return client;
            }
            catch (Exception)
            {
                client?.Dispose();
                throw;
            }
        }
    }
}