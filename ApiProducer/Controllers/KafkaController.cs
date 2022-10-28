using ApiProducer.Interfaces;
using ApiProducer.Models;
using Microsoft.AspNetCore.Mvc;
using Persistence.Models;
using Persistence.Mongo;

namespace ApiProducer.Controllers
{
    public class KafkaController : Controller
    {
        private readonly ILogger _logger;
        private readonly IKafkaContainer _kafkaContainer;
        private readonly IMongoRepository<Topic> _mongoRepository;

        public KafkaController(ILogger<KafkaController> logger, IKafkaContainer kafkaContainer, IMongoRepository<Topic> mongoRepository)
        {
            _logger = logger;
            _kafkaContainer = kafkaContainer;
            _mongoRepository = mongoRepository;
        }

        [Route("api/create")]
        [HttpPost]
        public async Task<IActionResult> CreateTopic(string topic)
        {
            _logger.LogInformation($"API: CreateTopic called with {topic}");
            try
            {
                await _kafkaContainer.CreateTopic(topic);
                Topic topicObj = new Topic()
                {
                    Name = topic,
                    Messages = new List<Message>()
                };
                await _mongoRepository.InsertOneAsync(topicObj);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            return Ok();
        }

        [Route("api/produce")]
        [HttpPost]
        public IActionResult Produce(KafkaMessageRequest request)
        {
            _logger.LogInformation($"API: Produce called with {request}");

            if (ModelState.IsValid)
            {
                try
                {
                    _kafkaContainer.ProduceMessage(request);
                }
                catch (UnknownTopicException ex)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
                return Ok();
            }
            _logger.LogInformation("Invalid Data Model - Bad Request");
            return StatusCode(StatusCodes.Status400BadRequest);
        }
    }
}
