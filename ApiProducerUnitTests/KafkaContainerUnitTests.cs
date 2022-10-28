using ApiProducer;
using ApiProducer.Models;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Models;
using Persistence.Mongo;
using System.Linq.Expressions;

namespace ApiProducerUnitTests
{
    public class KafkaContainerUnitTests
    {
        private Mock<ILogger<KafkaContainer>> _loggerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IMongoRepository<Topic>> _repositoryMock;
        private Mock<IAdminClient> _adminClientMock;
        private Mock<IProducer<string, string>> _producerMock;
        private KafkaContainer _sut;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<KafkaContainer>>();
            _configurationMock = new Mock<IConfiguration>();
            _repositoryMock = new Mock<IMongoRepository<Topic>>();
            _adminClientMock = new Mock<IAdminClient>();
            _producerMock = new Mock<IProducer<string, string>>();

            _sut = new KafkaContainer(_loggerMock.Object, _repositoryMock.Object, _producerMock.Object, _adminClientMock.Object);
        }

        [Test]
        public async Task Given_CreateMessageCalled_CallsAdminClient_Once()
        {
            string topicName = "test";
            await _sut.CreateTopic(topicName);

            _adminClientMock.Verify(x => x.CreateTopicsAsync(It.Is<TopicSpecification[]>(x => x[0].Name == topicName), null), Times.Once());
        }

        [Test]
        public void Given_ProduceMessageCalledWithUnknownTopic_ThrowsUnknownTopicException()
        {
            Topic? topic = null;
            _repositoryMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<Topic, bool>>>())).ReturnsAsync(topic);

            var request = new KafkaMessageRequest()
            {
                Message = "Test",
                Topic = "Test",
                User = "Test"
            };

            var ex = Assert.ThrowsAsync<UnknownTopicException>(() => _sut.ProduceMessage(request));

            Assert.That(ex.Message, Is.EqualTo($"Topic {request.Topic} has not yet been created"));
        }

        [Test]
        public async Task Given_ProduceMessageCalledWithValidTopic_CallsProduceAsync_Once()
        {
            Topic? topic = new Topic()
            {
                Id = new MongoDB.Bson.ObjectId(),
                Name = "Test",
                Messages = new List<Message>()
            };

            _repositoryMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<Topic, bool>>>())).ReturnsAsync(topic);

            KafkaMessageRequest request = new KafkaMessageRequest()
            {
                Topic = "Test",
                Message = "Test",
                User = "Test"
            };

            await _sut.ProduceMessage(request);

            _producerMock.Verify(x => x.ProduceAsync(request.Topic, It.Is<Message<string,string>>(x => x.Key == request.User && x.Value == request.Message), default(CancellationToken)), Times.Once());
        }
    }
}
