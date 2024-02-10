using System.Runtime.ConstrainedExecution;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace space
{
    public class AzureKeyVaultService : IAzureKeyVaultService{

        public AzureKeyVaultService() {

        }

        public String GetKeyVaultSecret(string secretName, string keyVaultUri) {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(5),
                    MaxRetries = 2,
                    Mode = RetryMode.Exponential
                }
            };
            var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential(), options);
            KeyVaultSecret secret = client.GetSecret(secretName);
            if (String.IsNullOrEmpty(secret.Value)) {
                return "";
            }
            return secret.Value;
        }
    }
}