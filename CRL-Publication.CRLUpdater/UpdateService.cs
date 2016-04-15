using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Infotecs.Pki.X509;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL_Publication.CRLUpdate
{
    using System.IO;
    using System.Linq;
    using NLog;   
    using MessageHandler;

    /// <summary>
    /// Класс, который описывает сервис обработки файлов отзывов сертификатов.
    /// </summary>
    public class UpdateService
    {
        /// <summary>
        /// Журнал событий.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Менеджер подключения и отправки данных на Rabbit сервер
        /// </summary>
        private RabbitConnectionManager _rabbitConnectionManager;

        /// <summary>
        /// Хранилище отзывов сертификатов
        /// </summary>
        private readonly X509Store _store;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UpdateService"/>.
        /// </summary>
        public UpdateService()
        {            
            this._store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
        }

        /// <summary>
        /// Запускает службу.
        /// </summary>
        public void Start()
        {            
            this._store.Open(OpenFlags.ReadWrite);
            this._rabbitConnectionManager = new RabbitConnectionManager();
            this._rabbitConnectionManager.ConfigureSettings(ConfigurationHelper.ServerAddress,
                                                            ConfigurationHelper.UserName,
                                                            ConfigurationHelper.UserPassword,
                                                            ConfigurationHelper.Exchange,
                                                            ConfigurationHelper.RoutingKey,
                                                            ConfigurationHelper.Queue);
            using (var connection = this._rabbitConnectionManager.CreateConnection())
            {
                if (connection != null)
                {
                    Logger.Trace("Получатель активирован.");
                    this._rabbitConnectionManager.RecievedMessage += this.ProcessFile;
                    this._rabbitConnectionManager.RecieveData(connection);
                }
                else
                {
                    Logger.Error("Rabbit сервер недоступен.");
                }
            }
        }

        /// <summary>
        /// Останавливает службу.
        /// </summary>
        public void Stop()
        {
            this._store.Close();
            this._rabbitConnectionManager.RecievedMessage -= this.ProcessFile;
            Logger.Trace("Получатель остановлен.");
        }

        /// <summary>
        /// Проверяет соединение.
        /// </summary>
        /// <returns>Возвращает true\false.</returns>
        public Boolean CheckConnection()
        {
            return true;
        }

        /// <summary>
        /// Обрабатывает файл отзывов сертификатов.
        /// </summary>
        /// <param name="crlFileName">Наименование CRL файла.</param>
        /// <param name="crl">Массив байт, представляющий файл отзывов сертификатов.</param>
        private void ProcessFile(Object sender, RecievedMessageEventArgs e)
        {
            Logger.Trace("Получен файл: \"{0}\".", e.CrlFileName);            
            try
            {
                var Crl = new X509Crl(e.Crl);
                this._store.Add(Crl);
                Logger.Info("Файл: \"{0}\" - установлен успешно.", e.CrlFileName);
            }
            catch(Exception ex)
            {
                Logger.Error("Файл: \"{0}\" - не установлен. Ошибка: {1}", e.CrlFileName, ex.ToString());
            }
            
        }
    }
}
