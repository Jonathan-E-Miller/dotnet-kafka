using ApiProducer.Models;

namespace ApiProducer.Interfaces
{
    public interface IKafkaContainer
    {
        Task CreateTopic(string topicName);
        Task ProduceMessage(KafkaMessageRequest request);
    }
}
