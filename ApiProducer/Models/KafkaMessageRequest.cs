using System.ComponentModel.DataAnnotations;

namespace ApiProducer.Models
{
    public class KafkaMessageRequest
    {
        [Required]
        public string User { get; set; }
        [Required]
        public string Topic { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
