using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iitrust.CRLPublication.Client.Properties;
using RabbitMQ.Client;
using MessageHandler;
using MessageHandler.Extensions;

namespace Iitrust.CRLPublication.Client.Models
{
    public class RabbitConnectionManager
    {
        /// <summary>
        /// Макет настроек для подключения к Rabbit серверу
        /// </summary>
        private IConnectionFactory _factory;

        /// <summary>
        /// Создает объект класса <see cref="RabbitConnectionManager"/>
        /// </summary>
        public RabbitConnectionManager()
        {
            CreateConnectionFactory();
        }

        /// <summary>
        /// Создает макет с настройками для подключения к Rabbit серверу
        /// </summary>
        public void CreateConnectionFactory()
        {
            this._factory = new ConnectionFactory()
            {
                HostName = ClientSettings.Default.URL,
                //UserName = ClientSettings.Default.USER,
                //Password = ClientSettings.Default.PASSWORD

            };
        }

        /// <summary>
        /// Отправляет файл в очередь
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileBytes"></param>
        public void SendData(String fileName, byte[] fileBytes)
        {
            var message = new Base64(fileName, fileBytes.ConvertToString()).Serialize();
            try
            {
                using (var connection = this._factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: ClientSettings.Default.EXCHANGE, type: "direct");
                    channel.QueueDeclare(ClientSettings.Default.QUEUE,
                        true,
                        false,
                        false,
                        null);

                    //var body = json.ConvertToBytes();

                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);

                    channel.BasicPublish(exchange: ClientSettings.Default.EXCHANGE,
                                         routingKey: ClientSettings.Default.ROUTING_KEY,
                                         basicProperties: properties,
                                         body: message.ConvertToBytes());

                    //Console.WriteLine(" [x] Sent json: {0}", json);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("***Error: " + e);
            }


        }
    }
}
