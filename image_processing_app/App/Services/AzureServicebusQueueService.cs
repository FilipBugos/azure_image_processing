using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace space
{
    public class AzureServicebusQueueService : IAzureServiceBusQueue
    {
        public AzureServicebusQueueService()
        {
        }

        public async Task SendMessage(string photoId, string connectionString, string queueName)
        {
            // Create a ServiceBusClient object using the connection string to the namespace.
            await using var client = new ServiceBusClient(connectionString);
            // Create a ServiceBusSender object by invoking the CreateSender method on the ServiceBusClient object, and specifying the queue name. 
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
                // Try again even if exception occurs
                // setup some repetition and circuit policy
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
