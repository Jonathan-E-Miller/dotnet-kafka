using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MvcConsumer.Controllers;
using MvcConsumer.Models;
using Persistence.Models;
using Persistence.Mongo;

namespace WebAppUnitTests
{
    public class HomeControllerUnitTests
    {
        private Mock<IMongoRepository<Topic>> _repositoryMock;
        private Mock<ILogger<HomeController>> _logMock;
        private HomeController _systemUnderTest;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<IMongoRepository<Topic>>();
            _logMock = new Mock<ILogger<HomeController>>();
            _systemUnderTest = new HomeController(_logMock.Object, _repositoryMock.Object);
        }

        [Test]
        public void Index_GivenDataExists_ViewModelIsCorrect()
        {
            _repositoryMock.Setup(x => x.All()).Returns(new List<Topic>()
            {
                new Topic()
                {
                    Id = new MongoDB.Bson.ObjectId(),
                    Name = "Test",
                    Messages = new List<Message>()
                    {
                        new Message()
                        {
                            User = "Test User",
                            Text = "Test Text",
                            ReceivedAt = DateTime.MinValue,
                        }
                    }
                }
            }.AsQueryable());

            IActionResult result = _systemUnderTest.Index();
            var model = ((ViewResult)result).Model as List<TopicViewModel>;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(model);
                Assert.That(model, Has.Count.EqualTo(1));
            });

        }

        [Test]
        public void Index_GivenDatabaseIsEmpty_ReturnsEmptyViewModel()
        {
            _repositoryMock.Setup(x => x.All()).Returns(new List<Topic>().AsQueryable());

            IActionResult result = _systemUnderTest.Index();
            var model = ((ViewResult)result).Model as List<TopicViewModel>;

            Assert.IsNotNull(model);
            Assert.That(model, Has.Count.EqualTo(0));
        }

        [Test]
        public async Task Topic_GivenTheTopicExists_ReturnsValidViewModel()
        {
            _repositoryMock.Setup(x => x.FindById(It.IsAny<string>())).ReturnsAsync(new Topic()
            {
                Id = new MongoDB.Bson.ObjectId(),
                Name = "Test",
                Messages = new List<Message>()
                {
                    new Message()
                    {
                        User = "Test User",
                        Text = "Test Text",
                        ReceivedAt = DateTime.MinValue,
                    }
                }
            });

            IActionResult result = await _systemUnderTest.Topic("test");
            var model = ((ViewResult)result).Model as TopicViewModel;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(model);
                Assert.That(model.Name, Is.EqualTo("Test"));
                Assert.That(model.Messages, Has.Count.EqualTo(1));
                Assert.That(model.Messages[0].Sender, Is.EqualTo("Test User"));
                Assert.That(model.Messages[0].Text, Is.EqualTo("Test Text"));
                Assert.That(model.Messages[0].Received, Is.EqualTo(DateTime.MinValue));
            });

        }
        [Test]
        public async Task Topic_GivenTheTopicDoesNotExist_ReturnsDefaultViewModel()
        {
            IActionResult result = await _systemUnderTest.Topic("1");
            var model = ((ViewResult)result).Model as TopicViewModel;

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(model);
                Assert.That(model.Name, Is.EqualTo("Unknow Topic"));
                Assert.That(model.Messages, Has.Count.EqualTo(0));
            });
        }

        [Test]
        public async Task Topic_GivenFindByIdThrowsException_ErroViewIsReturned()
        {
            _repositoryMock.Setup(x => x.FindById(It.IsAny<string>())).ThrowsAsync(new Exception());

            IActionResult result = await _systemUnderTest.Topic("1");
            var viewResult = result as ViewResult;
            Assert.That(viewResult?.ViewName, Is.EqualTo("Error"));
        }
    }
}