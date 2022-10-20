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
        private readonly IConfiguration _configuration;
        private readonly IMongoRepository<Topic> _repository;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IMongoRepository<Topic> repository)
        {
            _logger = logger;
            _configuration = configuration;
            _repository = repository;
        }

        public IActionResult Index()
        {
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
            Topic match = await _repository.FindById(topicId);

            TopicViewModel viewModel = new TopicViewModel()
            {
                Name = match.Name,
                Messages = match.Messages.Select(x => new MessageViewModel()
                {
                    Sender = x.User,
                    Text = x.Text,
                    Received = x.ReceivedAt
                }).ToList()
            };
            return View(viewModel);
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