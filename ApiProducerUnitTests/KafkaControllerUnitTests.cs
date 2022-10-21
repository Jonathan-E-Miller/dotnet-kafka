using ApiProducer.Controllers;
using ApiProducer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Models;
using Persistence.Mongo;

namespace ApiProducerUnitTests
{
    public class KafkaControllerUnitTests
    {
        private Mock<IMongoRepository<Topic>> _repository;
        private Mock<IKafkaContainer> _kafka;
        private KafkaController _systemUnderTest;

        [SetUp]
        public void Setup()
        {
            _repository = new Mock<IMongoRepository<Topic>>();
            _kafka= new Mock<IKafkaContainer>();
            var logging = new Mock<ILogger<KafkaController>>();
            _systemUnderTest = new KafkaController(logging.Object, _kafka.Object, _repository.Object);
        }

        [Test]
        public async Task Create_WhenCalledWithValidData_ReturnsOk()
        {
            string topic = "testTopic";
            IActionResult actionResult = await _systemUnderTest.CreateTopic(topic);
            var result = actionResult as StatusCodeResult;
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            });
        }

        [Test]
        public async Task Create_WhenDatabaseInsertFails_ReturnsBadRequest()
        {
            string topic = "test topic";
            _repository.Setup(x => x.InsertOneAsync(It.IsAny<Topic>())).ThrowsAsync(new Exception());
            IActionResult actionResult = await _systemUnderTest.CreateTopic(topic);
            var result = actionResult as ObjectResult;
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            });

        }

        [Test]
        public async Task Create_WhenKafkaCreationFails_ReturnsBadRequest()
        {
            string topic = "test topic";
            _kafka.Setup(x => x.CreateTopic(It.IsAny<string>())).ThrowsAsync(new Exception());
            IActionResult actionResult = await _systemUnderTest.CreateTopic(topic);
            var result = actionResult as ObjectResult;
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            });
        }
    }
}