namespace space{
    public interface IAzureBlobStorageService {
        public Task<String> UploadFileToStorage(Stream fileStream, string connectionString, string containerName);
        public String GetProcessedImage(string imageName, string connectionString, string containerName);
    }
}