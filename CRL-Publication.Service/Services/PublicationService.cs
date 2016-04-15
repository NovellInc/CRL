namespace Iitrust.CRLPublication.Service.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.Timers;
    using CRL_Publication.Db;
    using Infotecs.Pki.X509;
    using NLog;
    using Notification;    
    using MessageHandler;

    /// <summary>
    /// Класс, который описывает сервис обработки файлов отзывов сертификатов.
    /// </summary>
    public class PublicationService : IPublicationService
    {
        /// <summary>
        /// Журнал событий.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Таймер проверки задач в БД.
        /// </summary>
        private readonly Timer _taskTimer;

        /// <summary>
        /// Менеджер подключения и отправки данных на Rabbit сервер
        /// </summary>
        private RabbitConnectionManager _rabbitConnectionManager;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PublicationService"/>.
        /// </summary>
        public PublicationService()
        {
            this._taskTimer = new Timer(ConfigurationHelper.TimeInterval * 60000);
            this._taskTimer.Elapsed += this.TaskTimer_Elapsed;                 
        }

        /// <summary>
        /// Запускает службу.
        /// </summary>
        public void Start()
        {
            this.TaskTimer_Elapsed(null, null);
            this._taskTimer.Start();
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
                    Logger.Trace("Rabbit сервер недоступен.");
                }
            }
        }

        /// <summary>
        /// Останавливает службу.
        /// </summary>
        public void Stop()
        {
            this._taskTimer.Stop();
            this._rabbitConnectionManager.RecievedMessage -= this.ProcessFile;
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
            Logger.Trace("Получен на обработку файл: \"{0}\".", e.CrlFileName);

            using (var db = new CrlModelContainer())
            {
                try
                {
                    if (this.UpdateDb(db, e.CrlFileName, e.Crl))
                    {
                        this.UploadFileToFtp(e.CrlFileName, e.Crl);
                    }
                }
                catch (Exception exception)
                {
                    var message = String.Format("Во время обработки файла возникла ошибка: {0}.", exception.Message);
                    Logger.Error(message);
                    var crlEntity = db.CrlEntities.Local.FirstOrDefault(c => c.Name == e.CrlFileName);
                    if (crlEntity != null)
                    {
                        crlEntity.EventLog.Add(new EventLog
                        {
                            EventResult = EventResult.Error,
                            Message = message,
                            CrlEntity = crlEntity,
                            CrlEntityId = crlEntity.Id,
                            EventDate = DateTime.Now
                        });
                    }

                    throw new FaultException(exception.Message);
                }
                finally
                {
                    db.SaveChanges();
                }
            }

            Logger.Info("Файл: \"{0}\" - обработан успешно.", e.CrlFileName);
        }

        /// <summary>
        /// Обновляет информацию в БД.
        /// </summary>
        /// <param name="db">Контейнер БД.</param>
        /// <param name="crlFileName">Наименование CRL файла.</param>
        /// <param name="crl">Массив байт, представляющий файл отзывов сертификатов.</param>
        /// <returns>True - БД обновлена, False - изменений в БД не произведено.</returns>
        private Boolean UpdateDb(CrlModelContainer db, String crlFileName, Byte[] crl)
        {
            Logger.Trace("Начинаем проверку файла: \"{0}\" в БД.", crlFileName);

            var databaseCrl = db.CrlEntities.FirstOrDefault(c => c.Name == crlFileName);
            String file = Convert.ToBase64String(crl);
            String message;
            String previousCrl = null;
            EventResult result;

            if (databaseCrl == null)
            {
                Logger.Warn("Запись в БД не найдена. Производим запись в БД.");
                databaseCrl = new CrlEntity
                {
                    Name = crlFileName
                };

                db.CrlEntities.Add(databaseCrl);
                message = "Запись добавлена в БД.";
                result = EventResult.Add;
            }
            else
            {
                if (databaseCrl.File == file)
                {
                    Logger.Info("Запись в БД найдена, но обновление файлу не требуется.");
                    return false;
                }

                message = "Запись обновлена в БД.";
                previousCrl = databaseCrl.File;
                result = EventResult.Update;
            }

            var crlFile = new X509Crl(crl);
            databaseCrl.File = file;
            databaseCrl.NextUpdate = crlFile.NextUpdate;
            if (databaseCrl.MessageTask == null)
            {
                databaseCrl.MessageTask = new MessageTask
                {
                    CrlEntity = databaseCrl
                };
            }

            databaseCrl.MessageTask.SendingTime = databaseCrl.NextUpdate.AddHours(-8);

            Logger.Info(message);
            databaseCrl.EventLog.Add(new EventLog
            {
                EventResult = result,
                Message = message,
                CrlEntity = databaseCrl,
                CrlEntityId = databaseCrl.Id,
                PreviousCrlFile = previousCrl,
                EventDate = DateTime.Now
            });

            return true;
        }

        /// <summary>
        /// Загружает файл на FTP.
        /// </summary>
        /// <param name="crlFileName">Наименование CRL файла.</param>
        /// <param name="crl">Массив байт, представляющий файл отзывов сертификатов.</param>
        private void UploadFileToFtp(String crlFileName, Byte[] crl)
        {
            var ftpHost = String.IsNullOrEmpty(ConfigurationHelper.FtpPort)
                ? ConfigurationHelper.FtpAddress
                : String.Format("{0}:{1}", ConfigurationHelper.FtpAddress, ConfigurationHelper.FtpPort);
            var ftpUrl = String.Format(
                "ftp://{0}/uc/{1}",
                ftpHost,
                crlFileName);
            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.Credentials = new NetworkCredential(ConfigurationHelper.FtpUser, ConfigurationHelper.FtpPassword);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(crl, 0, crl.Length);
            }

            var response = (FtpWebResponse)request.GetResponse();
            Logger.Trace("Загрузка файла на фтп завершена. Статус: {0}", response.StatusDescription);
            response.Close();
        }

        /// <summary>
        /// Обрабатывает событие, которое происходит по истечению интервала времени.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Данные для события.</param>
        private void TaskTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Logger.Trace("Проверка времени обновления CRL.");
            try
            {
                using (var db = new CrlModelContainer())
                {
                    foreach (var messageTask in db.MessageTasks)
                    {
                        var interval = messageTask.SendingTime - DateTime.Now;
                        if (interval.TotalSeconds >= 0 && interval.TotalSeconds <= 60)
                        {
                            String message = String.Format(
                                "Необходимо обновить список отзывов сертификатов - \"{0}\".",
                                messageTask.CrlEntity.Name);
                            MailClient.Send(
                                message,
                                new MemoryStream(Convert.FromBase64String(messageTask.CrlEntity.File)),
                                messageTask.CrlEntity.Name);
                            SmsClient.Send(message);
                            Logger.Warn(
                                "Списку отзывов {0} необходимо обновление. Отправлено уведомление на почту и СМС.",
                                messageTask.CrlEntity.Name);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Возникла критическая ошибка: {0}", exception.Message);
                throw;
            }

            Logger.Trace("Проверка завершена.");
        }
    }
}