using System;
using MessageHandler.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MessageHandler
{
    /// <summary>
    /// Класс осуществляет соединение получателя/отправителя с сервером и производит прием/отправку данных
    /// </summary>
    public class RabbitConnectionManager
    {
        /// <summary>
        /// Адрес, по которому доступен Rabbit сервер.
        /// </summary>
        private String Url;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        private String UserName;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        private String Password;
       
        /// <summary>
        /// Точка обмена
        /// </summary>
        private String Exchange;

        /// <summary>
        /// Ключ маршрутизации
        /// </summary>
        private String RoutingKey;

        /// <summary>
        /// Имя очереди
        /// </summary>
        private String Queue;

        /// <summary>
        /// Инициализирует объект макета соединения
        /// </summary>
        private ConnectionFactory _connectionFactory;

        /// <summary>
        /// Состояние соединения с Rabbit сервером
        /// </summary>
        private Boolean _connected;

        /// <summary>
        /// Инициализирует экземпляр класса с настройками по умолчанию
        /// </summary>
        public RabbitConnectionManager()
        {
        }

        /// <summary>
        /// Создает макет соединения
        /// </summary>
        public void CreateConnectionFactory()
        {             
            this._connectionFactory = new ConnectionFactory()
            {
                HostName = Url,
                UserName = UserName,
                Password = Password
            };
        }

        /// <summary>
        /// Задает настройки для подключения
        /// </summary>
        /// <param name="url">Задает адрес Rabbit сервера</param>
        /// <param name="userName">Задает имя пользователя</param>
        /// <param name="password">Задает пароль пользователя</param>
        /// <param name="exchange">Задает точку обмена</param>
        /// <param name="routingKey">Задает ключ маршрутизации</param>
        /// <param name="queue">Задает имя очереди</param>
        /// <param name="targetDirectory">Задает целевую папку</param>
        public void ConfigureSettings(String url, String userName, String password, String exchange, String routingKey, String queue)
        {
            this._connected = false;
            Url = url;
            UserName = userName;
            Password = password;
            Exchange = exchange;
            RoutingKey = routingKey;
            Queue = queue;
        }

        /// <summary>
        /// Создание соединения с Rabbit сервером
        /// </summary>
        /// <returns>
        /// Возвращает объект соединения, если соединение установлено
        /// </returns>
        public IConnection CreateConnection()
        {
            CreateConnectionFactory();
            try
            {
                this._connected = true;
                return this._connectionFactory.CreateConnection();                 
            }
            catch (Exception e)
            {
                this._connected = false;
                return null;
            }
        }

        /// <summary>
        /// Проверка состояния соединения
        /// </summary>
        /// <returns></returns>
        public Boolean CheckConnection()
        {
            return this._connected;
        }

        /// <summary>
        /// Отправляет файл на Rabbit сервер
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="dataBytes">Массив байт данных</param>
        /// <param name="fileName">Имя файла</param>
        public void SendData(IConnection connection, byte[] dataBytes, String fileName)
        {
            var base64Data = new Base64(fileName, dataBytes);
            String json = base64Data.Serialize();

            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: Exchange, type: "fanout");
                //channel.QueueDeclare(Queue, true, false, false, null);

                var body = json.ConvertToBytes();

                var properties = channel.CreateBasicProperties();
                properties.SetPersistent(true);

                channel.BasicPublish(exchange: Exchange,
                                     routingKey: RoutingKey,
                                     basicProperties: properties,
                                     body: body);
            } 
        }

        /// <summary>
        /// Событие происходит при получении сообщения
        /// </summary>
        public event EventHandler<RecievedMessageEventArgs> RecievedMessage;

        /// <summary>
        /// Оповещение подписчиков о происшествии события получения файла
        /// </summary>
        /// <param name="e">Информация о полученном сообщении</param>
        protected void OnRecievedMessage(RecievedMessageEventArgs e)
        {
            EventHandler<RecievedMessageEventArgs> temp = RecievedMessage;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        /// <summary>
        /// Вызов события
        /// </summary>
        /// <param name="crlFileName">Имя CRL файла</param>
        /// <param name="crl">Содержимое CRL файла</param>
        public void RecieveMessage(String crlFileName, Byte[] crl)
        {
            RecievedMessageEventArgs e = new RecievedMessageEventArgs(crlFileName, crl);
            OnRecievedMessage(e);
        }

        /// <summary>
        /// Принимает файл от Rabbit сервера
        /// </summary>
        public void RecieveData(IConnection connection)
        {
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: Exchange, type: "fanout");
                //channel.QueueDeclare(Queue, true, false, false, null);

                //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                var q = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: q,
                                  exchange: Exchange,
                                  routingKey: RoutingKey);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var message = ea.Body.ConvertToString().Deserialize<Base64>();
                    RecieveMessage(message.FileName, message.Data);
                };
                channel.BasicConsume(queue: q,
                                     noAck: true,
                                     consumer: consumer);
                while (true) { }
            }
        }

    }
}
