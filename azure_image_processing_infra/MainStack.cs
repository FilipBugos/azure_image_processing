using System;
using System.Runtime.ConstrainedExecution;
using Azure.Identity;
using Constructs;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azurerm.AppService;
using HashiCorp.Cdktf.Providers.Azurerm.AppServicePlan;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermStorageAccount;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermStorageBlob;
using HashiCorp.Cdktf.Providers.Azurerm.DataAzurermStorageContainer;
using HashiCorp.Cdktf.Providers.Azurerm.FunctionApp;
using HashiCorp.Cdktf.Providers.Azurerm.KeyVault;
using HashiCorp.Cdktf.Providers.Azurerm.KeyVaultAccessPolicy;
using HashiCorp.Cdktf.Providers.Azurerm.KeyVaultSecret;
using HashiCorp.Cdktf.Providers.Azurerm.LinuxWebApp;
using HashiCorp.Cdktf.Providers.Azurerm.Provider;
using HashiCorp.Cdktf.Providers.Azurerm.ResourceGroup;
using HashiCorp.Cdktf.Providers.Azurerm.ServicebusNamespace;
using HashiCorp.Cdktf.Providers.Azurerm.ServicebusQueue;
using HashiCorp.Cdktf.Providers.Azurerm.ServicePlan;
using HashiCorp.Cdktf.Providers.Azurerm.StorageAccount;
using HashiCorp.Cdktf.Providers.Azurerm.StorageBlob;
using HashiCorp.Cdktf.Providers.Azurerm.StorageContainer;
using HashiCorp.Cdktf.Providers.Azurerm.UserAssignedIdentity;
using HashiCorp.Cdktf.Providers.Azurerm.VirtualNetwork;


namespace MyCompany.MyApp
{
    class MainStack : TerraformStack
    {
        public MainStack(Construct scope, string id) : base(scope, id)
        {
            var tenantId = Environment.GetEnvironmentVariable("TENANT_ID") ?? "675348b9-9310-4777-b6c9-28067ebb3e91";
            var objectId = Environment.GetEnvironmentVariable("OBJECT_ID") ?? "675348b9-9310-4777-b6c9-28067ebb3e91";
            // create resource group
            
            var provider = new AzurermProvider(this, "AzureRm", new AzurermProviderConfig {
                Features = new AzurermProviderFeatures(),
                SkipProviderRegistration = true
            });

            var resourceGroupName = "rg-imageProcessing-development-infra-westEurope";
            var resourceGroup = new ResourceGroup(this, "AzureRg", new ResourceGroupConfig{
                Name = resourceGroupName,
                Location = "westEurope"
            });

            var appServicePlanId = "example";
            var servicePlan = new ServicePlan(this, appServicePlanId, new ServicePlanConfig{
                ResourceGroupName = resourceGroup.Name,
                Name = "test_service_plan_for_testing_purposes",
                SkuName = "B1",
                OsType = "Linux",
                Location = "westeurope",

            });

            var storageAccount = new StorageAccount(this, "storage account", new StorageAccountConfig{
                Name = "saimageprocessingstorage",
                ResourceGroupName = resourceGroup.Name,
                AccountReplicationType = "LRS",
                AccountTier = "Standard",
                Location = "westeurope"
            });

            var storageContainer = new StorageContainer(this, "storage container", new StorageContainerConfig{
                StorageAccountName = storageAccount.Name,
                Name = "scuserfile",
                ContainerAccessType = "blob",
                DependsOn = new ITerraformDependable[] {storageAccount}
            });
            
            var webapp = new LinuxWebApp(this, "AppService", new LinuxWebAppConfig{
                ServicePlanId = servicePlan.Id,
                ResourceGroupName = resourceGroup.Name,
                Location = "westeurope",
                Name = "TestWebAppForTestingPurposesOnly",
                HttpsOnly = true,
                SiteConfig = new LinuxWebAppSiteConfig{
                    AlwaysOn = false,
                    ApplicationStack = new LinuxWebAppSiteConfigApplicationStack {
                        DotnetVersion = "6.0"
                    }
                },
                Identity = new LinuxWebAppIdentity{
                    Type = "SystemAssigned"
                },
            });

            var keyVault = new KeyVault(this, "Key vault", new KeyVaultConfig{
                Name = "kv-image-processing",
                ResourceGroupName = resourceGroup.Name,
                Location = "westeurope",
                SkuName = "standard",
                TenantId = tenantId,
                //PublicNetworkAccessEnabled = false,
            });

            var deploymentAccessPolicy = new KeyVaultAccessPolicyA(this, "Deployment key vault access policy", new KeyVaultAccessPolicyAConfig{
                KeyVaultId = keyVault.Id,
                ObjectId = objectId,
                SecretPermissions = new string[] {"Get", "Set", "Delete", "List"},
                TenantId = tenantId
            });

            var accessPolicy_WebApp = new KeyVaultAccessPolicyA(this, "Web app key vault access policy", new KeyVaultAccessPolicyAConfig{
                KeyVaultId = keyVault.Id,
                ObjectId = webapp.Identity.PrincipalId,
                SecretPermissions = new string[] {"Get", "Set"},
                TenantId = tenantId
            });

            var keyVaultSecret = new KeyVaultSecret(this, "Key vault secret", new KeyVaultSecretConfig{
                KeyVaultId = keyVault.Id,
                Name = "secret",
                Value = storageAccount.PrimaryConnectionString,
                DependsOn = new ITerraformDependable[] {storageAccount, deploymentAccessPolicy, accessPolicy_WebApp}
            });

            var servicebusNamespace = new ServicebusNamespace(this, "Service bus namespace", new ServicebusNamespaceConfig{
                Location = "westeurope",
                Name = "ServicebusNamespaceJustForTest2024",
                ResourceGroupName = resourceGroup.Name,
                Sku = "Basic",
                DependsOn = new ITerraformDependable[] {resourceGroup}
            });

            var servicebusQueue = new ServicebusQueue(this, "Servicebus queue", new ServicebusQueueConfig{
                Name = "ServiceBusQueue",
                NamespaceId = servicebusNamespace.Id
            });

            var serviceBusSecret = new KeyVaultSecret(this, "Service bus secret", new KeyVaultSecretConfig{
                KeyVaultId = keyVault.Id,
                Name = "serviceBusSecret",
                Value = servicebusNamespace.DefaultPrimaryConnectionString,
                DependsOn = new ITerraformDependable[] {servicebusQueue, deploymentAccessPolicy, accessPolicy_WebApp}
            });

            var processingFunction = new FunctionApp(this, "Image processing function", new FunctionAppConfig{
                AppServicePlanId = servicePlan.Id,
                Location = "westeurope",
                Name = "process-color-picture-to-black-and-white",
                ResourceGroupName = resourceGroup.Name,
                StorageAccountName = storageAccount.Name,
                StorageAccountAccessKey = storageAccount.PrimaryAccessKey,
                SiteConfig = new FunctionAppSiteConfig{
                    AlwaysOn = false,
                    DotnetFrameworkVersion = "v6.0"
                },
            });

            // Define 
        }
    }
}