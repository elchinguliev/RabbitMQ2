using Consumer.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace Consumer.Controllers
{
    public class HomeController : Controller
    {
        //public ActionResult Index()
        //{
        //    return View(new MessageViewModel());
        //}

        [HttpPost]
        public ActionResult Index(string selectedOption="orange")
        {
            var messages = ReceiveMessages(selectedOption);
            ViewBag.Messages = messages;
            return View();
        }

        private List<string> ReceiveMessages(string selectedOption)
        {
            var messages = new List<string>();

            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://wkrhegxa:6UytImCBsgpRvzEKMSy4Ji-T68l7kOxl@roedeer.rmq.cloudamqp.com/wkrhegxa") // RabbitMQ connection URI
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_exchange", type: ExchangeType.Topic);
                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName, exchange: "topic_exchange", routingKey: selectedOption);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    messages.Add(message);
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }

            return messages;
        }
    }
}
