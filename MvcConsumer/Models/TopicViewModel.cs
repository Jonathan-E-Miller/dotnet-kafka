namespace MvcConsumer.Models
{
    public class TopicViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<MessageViewModel> Messages { get; set; }
        public string LastReceived { get; set; }
    }
}
