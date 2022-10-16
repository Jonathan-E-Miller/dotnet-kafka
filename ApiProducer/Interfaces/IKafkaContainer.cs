using ApiProducer.Models;

namespace ApiProducer.Interfaces
{
    public interface IKafkaContainer
    {
        Task CreateTopic(string topicName);
        void ProduceMessage(KafkaMessageRequest request);
    }
}
