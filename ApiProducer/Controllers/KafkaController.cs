﻿using ApiProducer.Interfaces;
using ApiProducer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiProducer.Controllers
{
    public class KafkaController : Controller
    {
        private readonly ILogger _logger;
        private readonly IKafkaContainer _kafkaContainer;

        public KafkaController(ILogger<KafkaController> logger, IKafkaContainer kafkaContainer)
        {
            _logger = logger;
            _kafkaContainer = kafkaContainer;
        }

        [Route("api/create")]
        [HttpPost]
        public async Task<IActionResult> CreateTopic(string topic)
        {
            _logger.LogInformation($"API: CreateTopic called with {topic}");
            try
            {
                await _kafkaContainer.CreateTopic(topic);
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