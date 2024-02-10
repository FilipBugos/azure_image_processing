namespace space{
    public interface IAzureKeyVaultService {
        public String GetKeyVaultSecret(string secretName, string keyVaultUri);
    }
}