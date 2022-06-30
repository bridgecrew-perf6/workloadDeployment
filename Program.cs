// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;

namespace DeployUsingARMTemplate
{
    public class PhpWorkload
    {
        private static ResourceIdentifier? _resourceGroupId = null;

        /**
         * Using .NET SDK APIs for deploying resources using an ARM template.
         */
        public static async Task RunSample(ArmClient client)
        {
            var rgName = "phpdeploy"; // change the value here for resource group name
            var deploymentName = "test4"; // change the value here for arm deployment name
            try
            {
                //=============================================================
                // Create resource group.
                Console.WriteLine($"Creating a resource group with name: {rgName}");

                var subscription = await client.GetDefaultSubscriptionAsync();
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS2));
                var resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;

                Console.WriteLine($"subscription using: {subscription}");
                Console.WriteLine($"Created a resource group: {_resourceGroupId}");
                //=============================================================
                // Create a deployment for an Azure App Service via an ARM
                // template.

                Console.WriteLine($"Starting a deployment for an Azure App Service: {deploymentName}");
                var templateContent = File.ReadAllText("wptemplate.json").TrimEnd();
                var deploymentContent = new ArmDeploymentContent(new ArmDeploymentProperties(ArmDeploymentMode.Incremental)
                {
                    Template = BinaryData.FromString(templateContent),
                    Parameters = BinaryData.FromObjectAsJson(new
                    {
                        subscriptionId = new
                        {
                            value = "b1275574-d623-476f-aa26-c5322279c1c7"
                        },
                        resourceGroupName = new
                        {
                            value = rgName
                        },
                        resourceName = new
                        {
                            value = deploymentName
                        },
                    })
                });
                // we do not need the response of this deployment, therefore we just wait the deployment to complete here and discard the response.
                await resourceGroup.GetArmDeployments().CreateOrUpdateAsync(WaitUntil.Completed, deploymentName, deploymentContent);

                Console.WriteLine($"Completed the deployment: {deploymentName}");
            }
            finally
            {
                Console.WriteLine("deployment finished");
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var credential = new DefaultAzureCredential();

                var subscriptionId = "b1275574-d623-476f-aa26-c5322279c1c7";
                // we can also use `new ArmClient(credential)` here, and the default subscription will be the first subscription in your list of subscription
                var client = new ArmClient(credential, subscriptionId);

                await RunSample(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}