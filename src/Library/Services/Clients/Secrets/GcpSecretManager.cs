using Google.Cloud.SecretManager.V1;

namespace Library.Services._Clients.Secrets
{
    public class GcpSecretManager
    {
        private readonly string _projectId;
        private readonly string _secretVersionDbUserPassword = "1";

        public GcpSecretManager(string projectId)
        {
            _projectId = projectId;
        }

        public string GetSecret(string secretId)
        {
            SecretManagerServiceClient client = SecretManagerServiceClient.Create();
            SecretVersionName secretVersionName = new SecretVersionName(_projectId, secretId, _secretVersionDbUserPassword);
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
            var secretValue = result.Payload.Data.ToStringUtf8();
            return secretValue;
        }
    }
}
