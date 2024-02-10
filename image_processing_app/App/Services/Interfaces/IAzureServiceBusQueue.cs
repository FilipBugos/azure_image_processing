namespace space {
    public interface IAzureServiceBusQueue {
        public Task SendMessage(string message, string connectionString, string queueName);
    }
}