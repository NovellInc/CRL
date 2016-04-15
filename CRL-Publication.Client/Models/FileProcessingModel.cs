namespace Iitrust.CRLPublication.Client.Models
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Service = PublicationServiceReference.PublicationServiceClient;
    using MessageHandler;

    /// <summary>
    /// Состояние обработки.
    /// </summary>
    public enum ProcessingStatus
    {
        /// <summary>
        /// Статус - Остановлен.
        /// </summary>
        Stop = 0,

        /// <summary>
        /// Статус - Запущен.
        /// </summary>
        Start = 1
    }

    /// <summary>
    /// Модель обработки файлов отзывов сертификатов.
    /// </summary>
    public class FileProcessingModel
    {   
        /// <summary>
        /// Настройки приложения.
        /// </summary>
        private readonly SettingsModel _settings;

        /// <summary>
        /// Отслеживание уведомлений от файловой системы.
        /// </summary>
        private readonly FileSystemWatcher _fileSystemWatcher;

        /// <summary>
        /// Дата и время последнего срабатывания события изменения файла.
        /// </summary>
        private DateTime _lastTimeFileWatcherEventRaised;

        /// <summary>
        /// Менеджер подключения и отправки данных на Rabbit сервер
        /// </summary>
        private RabbitConnectionManager _rabbitConnectionManager;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FileProcessingModel"/>.
        /// </summary>
        /// <param name="settings">Настройки приложения</param>
        public FileProcessingModel(SettingsModel settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException();
            }                 
            this._settings = settings;

            this._rabbitConnectionManager = new RabbitConnectionManager();
            this._rabbitConnectionManager.ConfigureSettings(this._settings.ServerAddress,
                                                            this._settings.UserName,
                                                            this._settings.Password,
                                                            this._settings.Exchange,
                                                            this._settings.RoutingKey,
                                                            this._settings.Queue);

            this._fileSystemWatcher = new FileSystemWatcher(this._settings.DirectoryPath, SettingsModel.FileExtension)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess
            };
            this._fileSystemWatcher.Created += this.FileSystemWatcher_Created;
            this._fileSystemWatcher.Changed += this.FileSystemWatcher_Changed;
            this._fileSystemWatcher.Renamed += this.FileSystemWatcher_Renamed;
            this._fileSystemWatcher.Deleted += this.FileSystemWatcher_Deleted;
        }

        /// <summary>
        /// Происходит при создании файла.
        /// </summary>
        public event FileSystemEventHandler Created;

        /// <summary>
        /// Происходит при изменении файла.
        /// </summary>
        public event FileSystemEventHandler Changed;

        /// <summary>
        /// Происходит при изменении файла.
        /// </summary>
        public event RenamedEventHandler Renamed;

        /// <summary>
        /// Происходит при изменении файла.
        /// </summary>
        public event FileSystemEventHandler Deleted;

        /// <summary>
        /// Запускает слежение за файловой системой.
        /// </summary>
        public void Start()
        {
            var collection = this._settings.CorrectCollection();
            if (collection != null)
            {
                foreach (var crlModel in collection)
                {
                    this.FileSystemWatcher_Created(
                        this,
                        new FileSystemEventArgs(
                            WatcherChangeTypes.Created,
                            this._settings.DirectoryPath,
                            crlModel.FileName));
                }
            }

            foreach (var crlModel in this._settings.CrlModels)
            {
                this.ProcessFile(this._settings.DirectoryPath, crlModel.FileName);
            }

            this._fileSystemWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Останавливает слежение за файловой системой.
        /// </summary>
        public void Stop()
        {
            this._fileSystemWatcher.EnableRaisingEvents = false;
        }

        /// <summary>
        /// Обрабатывает событие изменения файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            var totalMs = DateTime.Now.Subtract(this._lastTimeFileWatcherEventRaised).TotalMilliseconds;
            if (totalMs < 700)
            {
                return;
            }

            Thread.Sleep(TimeSpan.FromSeconds(10));
            this._lastTimeFileWatcherEventRaised = DateTime.Now;
            this.ProcessFile(e.FullPath.Replace(e.Name, string.Empty), e.Name);

            if (this.Changed != null)
            {
                this.Changed(sender, e);
            }
        }

        /// <summary>
        /// Обрабатывает файл.
        /// </summary>
        /// <param name="fullPath">Полный путь для файла.</param>
        /// <param name="name">Имя файла.</param>
        private void ProcessFile(string fullPath, string name)
        {
            var crlModel = this._settings.CrlModels.FirstOrDefault(c => c.FileName == name);
            if (crlModel == null)
            {
                return;
            }

            string filePath = Path.Combine(fullPath, name);
            byte[] fileBytes;
            try
            {
                fileBytes = File.ReadAllBytes(filePath);
            }
            catch (IOException)
            {
                string destPath = Path.Combine(fullPath, "copy_" + name);
                File.Copy(filePath, destPath);
                fileBytes = File.ReadAllBytes(destPath);
                File.Delete(destPath);
            }

            this.ProcessFile(fileBytes, crlModel.CrlName);
        }

        /// <summary>
        /// Обрабатывает файл.
        /// </summary>
        /// <param name="fileBytes">Массив байт файла.</param>
        /// <param name="crlName">Имя файла CRL.</param>
        private void ProcessFile(byte[] fileBytes, string crlName)
        {            
            using (var connection = _rabbitConnectionManager.CreateConnection())
            {
                _rabbitConnectionManager.SendData(connection, fileBytes, crlName);
            }
        }

        /// <summary>
        /// Обрабатывает событие создания файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (this.Created != null && !e.Name.Contains("copy_"))
            {
                this.Created(sender, e);
            }
        }

        /// <summary>
        /// Обрабатывает событие удаление файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (this.Deleted != null && !e.Name.Contains("copy_"))
            {
                this.Deleted(sender, e);
            }
        }

        /// <summary>
        /// Обрабатывает событие переименование файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (this.Renamed != null)
            {
                this.Renamed(sender, e);
            }
        }
    }
}
