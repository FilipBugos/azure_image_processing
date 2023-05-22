using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace space
{
    public class ServiceBusService
    {
        private readonly IConfiguration _configuration;
        public ServiceBusService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task method(string photoId)
        {
            // Create a ServiceBusClient object using the connection string to the namespace.
            var connectionString = _configuration.GetValue<String>("ConnectionStrings:MessageQueue"); 
            await using var client = new ServiceBusClient(connectionString);

            // Create a ServiceBusSender object by invoking the CreateSender method on the ServiceBusClient object, and specifying the queue name. 
            var queueName = _configuration.GetValue<String>("ConnectionStrings:QueueName");
            await using ServiceBusSender sender = client.CreateSender(queueName);

            // Create a new message to send to the queue.
            var objectSend = new SendObject()
            {
                PhotoId = photoId,
                MessageId = Guid.NewGuid().ToString()
            };
            var message = JsonSerializer.Serialize(objectSend);
            try
            {
                var messageContent = new ServiceBusMessage(JsonSerializer.Serialize(message));

                // Send the message to the queue.
                await sender.SendMessageAsync(messageContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing message: ", message, " with ex: ", ex.Message);
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
