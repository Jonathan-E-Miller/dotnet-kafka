using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using MvcConsumer.Models;
using Persistence.Models;
using Persistence.Mongo;
using System.Diagnostics;

namespace MvcConsumer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMongoRepository<Topic> _repository;

        public HomeController(ILogger<HomeController> logger, IMongoRepository<Topic> repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("HomeController.Index called");
            List<Topic> all = _repository.All().ToList();
            List<TopicViewModel> topics = all.Select(x => new TopicViewModel()
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Messages = x.Messages.Select(m => new MessageViewModel()
                {
                    Text = m.Text,
                    Received = m.ReceivedAt,
                    Sender = m.User
                }).ToList(),
                LastReceived = x.Messages.Any() ? x.Messages.OrderByDescending(x => x.ReceivedAt).First().ReceivedAt.ToString() : "No messages"
            }).ToList();
                
            return View(topics);
        }

        public async Task<IActionResult> Topic(string topicId)
        {
            try
            {
                _logger.LogInformation($"HomeController.Topic called for {topicId}");
                Topic match = await _repository.FindById(topicId);

                TopicViewModel viewModel = new TopicViewModel()
                {
                    Name = "Unknow Topic",
                    Messages = new List<MessageViewModel>(),
                    LastReceived = DateTime.MinValue.ToString()
                };

                if (match != null)
                {
                    viewModel = new TopicViewModel()
                    {
                        Name = match.Name,
                        Messages = match.Messages.Select(x => new MessageViewModel()
                        {
                            Sender = x.User,
                            Text = x.Text,
                            Received = x.ReceivedAt
                        }).ToList()
                    };
                }

                return View(viewModel);
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            string requestId = Activity.Current?.Id ?? (HttpContext?.TraceIdentifier ?? "Unknown Request ID");

            return View("Error", new ErrorViewModel()
            {
              RequestId = requestId
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}