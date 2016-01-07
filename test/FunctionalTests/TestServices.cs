﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Server.Testing;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace EntropyTests
{
    public static class TestServices
    {
        public static void LogResponseOnFailedAssert(this ILogger logger, HttpResponseMessage response, string responseText, Action assert)
        {
            try
            {
                assert();
            }
            catch (XunitException)
            {
                logger.LogWarning(response.ToString());
                if (!string.IsNullOrEmpty(responseText))
                {
                    logger.LogWarning(responseText);
                }
                throw;
            }
        }

        public static async Task RunSiteTest(string siteName, ServerType serverType, RuntimeFlavor runtimeFlavor, RuntimeArchitecture architecture, string applicationBaseUrl,
            Func<HttpClient, ILogger, CancellationToken, Task> validator)
        {
            var logger = new LoggerFactory()
                            .AddConsole()
                            .CreateLogger(string.Format("{0}:{1}:{2}:{3}", siteName, serverType, runtimeFlavor, architecture));

            using (logger.BeginScope("RunSiteTest"))
            {
                var deploymentParameters = new DeploymentParameters(GetPathToApplication(siteName), serverType, runtimeFlavor, architecture)
                {
                    ApplicationBaseUriHint = applicationBaseUrl,
                    SiteName = "HttpTestSite",
                };

                using (var deployer = ApplicationDeployerFactory.Create(deploymentParameters, logger))
                {
                    var deploymentResult = deployer.Deploy();
                    var httpClientHandler = new HttpClientHandler();
                    var httpClient = new HttpClient(httpClientHandler)
                    {
                        BaseAddress = new Uri(deploymentResult.ApplicationBaseUri),
                        Timeout = TimeSpan.FromSeconds(10)
                    };

                    await validator(httpClient, logger, deploymentResult.HostShutdownToken);
                }
            }
        }

        private static string GetPathToApplication(string applicationName)
        {
            return Path.GetFullPath(Path.Combine("..", "..", "samples", applicationName));
        }
    }
}
