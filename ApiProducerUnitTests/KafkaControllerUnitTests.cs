using ApiProducer;
using ApiProducer.Controllers;
using ApiProducer.Interfaces;
using ApiProducer.Models;
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

        [Test]
        public async Task Produce_WhenCalledWithInvalidModel_ReturnsBadRequest()
        {
            _systemUnderTest.ModelState.AddModelError("key", "error");
            IActionResult actionResult = await _systemUnderTest.Produce(new KafkaMessageRequest());
            var result = actionResult as StatusCodeResult;
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            });
        }

        [Test]
        public async Task Produce_GivenKafkaContainerThrowsUnknownTopicException_ReturnsInternalServerError()
        {
            string exceptionMessage = "test message";
            _kafka.Setup(x => x.ProduceMessage(It.IsAny<KafkaMessageRequest>())).Throws(new UnknownTopicException(exceptionMessage));
            IActionResult actionResult = await _systemUnderTest.Produce(new KafkaMessageRequest());
            var result = actionResult as ObjectResult;
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo(exceptionMessage));
            });

        }

        [Test]
        public async Task Produce_GivenKafkaContainerThrowsGeneralException_ReturnsInternalServerError()
        {
            string exceptionMessage = "test message";
            _kafka.Setup(x => x.ProduceMessage(It.IsAny<KafkaMessageRequest>())).Throws(new Exception(exceptionMessage));
            IActionResult actionResult = await _systemUnderTest.Produce(new KafkaMessageRequest());
            var result = actionResult as ObjectResult;
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
                Assert.That(result?.Value, Is.EqualTo(exceptionMessage));
            });

        }
    }
}