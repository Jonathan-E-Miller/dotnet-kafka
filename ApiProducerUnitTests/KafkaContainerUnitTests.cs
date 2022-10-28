using ApiProducer;
using ApiProducer.Models;
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
        private KafkaContainer _sut;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<KafkaContainer>>();
            _configurationMock = new Mock<IConfiguration>();
            _repositoryMock = new Mock<IMongoRepository<Topic>>();

            _sut = new KafkaContainer(_loggerMock.Object, _configurationMock.Object, _repositoryMock.Object);
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
    }
}
