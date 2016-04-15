namespace Iitrust.CRLPublication.Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using PublicationServiceReference;
    using Utilities;
    using Utilities.WPF;
    using MessageHandler;

    /// <summary>
    /// Модель настроек приложения.
    /// </summary>
    [Serializable]
    public class SettingsModel : Notifier, ISettingsModel
    {
        /// <summary>
        /// Расширение файла отзыва сертификатов.
        /// </summary>
        public static readonly String FileExtension = "*.crl";

        /// <summary>
        /// Наименование файла настроек.
        /// </summary>
        public static readonly String FileName = "Settings.xml";

        /// <summary>
        /// Путь к хранилищу настроек приложения.
        /// </summary>
        public static readonly String ProgramDataPath;

        /// <summary>
        /// Путь к каталогу с файлами отзывов сертификатов.
        /// </summary>
        private String _directoryPath;

        /// <summary>
        /// Список файлов отзывов сертификатов.
        /// </summary>
        private ExtendedObservableCollection<CrlModel> _crlModels;

        #region Настройки подключения

        /// <summary>
        /// Адрес сервера.
        /// </summary>
        private String _serverAddress;

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        private String _userName;

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        private String _password;

        /// <summary>
        /// Точка обмена.
        /// </summary>
        private String _exchange;

        /// <summary>
        /// Ключ маршрутизации.
        /// </summary>
        private String _routingKey;

        /// <summary>
        /// Имя очереди.
        /// </summary>
        private String _queue;

        #endregion

        /// <summary>
        /// Инициализирует статические переменные класса <see cref="SettingsModel"/>.
        /// </summary>
        static SettingsModel()
        {
            ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IITrust\\CRL-Publication\\Client");
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SettingsModel"/>.
        /// </summary>
        public SettingsModel()
        {
            this.CrlModels = new ExtendedObservableCollection<CrlModel>();
        }

        /// <summary>
        /// Событие сохранение настроек.
        /// </summary>
        public event EventHandler SettingsSaved;

        /// <summary>
        /// Получает или задает путь к каталогу с файлами отзывов сертификатов.
        /// </summary>
        public String DirectoryPath
        {
            get
            {
                return this._directoryPath;
            }

            set
            {
                this._directoryPath = value;
                this.NotifyPropertyChanged("DirectoryPath");
            }
        }

        /// <summary>
        /// Получает или задает список файлов отзывов сертификатов.
        /// </summary>
        public ExtendedObservableCollection<CrlModel> CrlModels
        {
            get
            {
                return this._crlModels;
            }

            set
            {
                this._crlModels = value;
                this.NotifyPropertyChanged("CrlModels");
            }
        }

        #region Настройки подключения к Rabbit серверу

        /// <summary>
        /// Получает или задает адрес сервера обработки CRL.
        /// </summary>
        public String ServerAddress
        {
            get
            {
                return this._serverAddress;
            }

            set
            {
                this._serverAddress = value;
                this.NotifyPropertyChanged("ServerAddress");
            }
        }

        /// <summary>
        /// Получает или задает имя пользователя для подключения к серверу обработки CRL.
        /// </summary>
        public String UserName
        {
            get
            {
                return this._userName;
            }
            set
            {
                this._userName = value;
                this.NotifyPropertyChanged("UserName");
            }
        }

        /// <summary>
        /// Получает или задает пароль пользователя для подключения к серверу обработки CRL.
        /// </summary>
        public String Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
                this.NotifyPropertyChanged("Password");
            }
        }

        /// <summary>
        /// Получает или задает точку обмена.
        /// </summary>
        public String Exchange
        {
            get
            {
                return this._exchange;
            }
            set
            {
                this._exchange = value;
                this.NotifyPropertyChanged("Exchange");
            }
        }

        /// <summary>
        /// Получает или задает ключ маршрутизации.
        /// </summary>
        public String RoutingKey
        {
            get
            {
                return this._routingKey;
            }
            set
            {
                this._routingKey = value;
                this.NotifyPropertyChanged("RoutingKey");
            }
        }

        /// <summary>
        /// Получает или задает имя очереди.
        /// </summary>
        public String Queue
        {
            get
            {
                return this._queue;
            }
            set
            {
                this._queue = value;
                this.NotifyPropertyChanged("Queue");
            }
        }

        #endregion

        /// <summary>
        /// Получает файлы отзывов запросов в указанной директории.
        /// </summary>
        /// <param name="directoryPath">Путь к каталогу с файлами отзывов.</param>
        /// <returns>Возвращает коллекцию файлов отзывов запросов.</returns>
        public static ExtendedObservableCollection<CrlModel> GetCrlModels(String directoryPath)
        {
            var crlModels = new ExtendedObservableCollection<CrlModel>();
            var directoryInfo = new DirectoryInfo(directoryPath);
            foreach (var fileInfo in directoryInfo.GetFiles(FileExtension, SearchOption.TopDirectoryOnly))
            {
                crlModels.Add(new CrlModel(fileInfo.FullName, fileInfo.Name));
            }

            return crlModels;
        }

        /// <summary>
        /// Открывает настройки.
        /// </summary>
        /// <returns>Настройки для приложения или null если файл не найден.</returns>
        public static SettingsModel Open()
        {
            return SerializeHelper<SettingsModel>.Deserialize(Path.Combine(ProgramDataPath, FileName));
        }

        /// <summary>
        /// Показывает диалог выбора каталога с файлами.
        /// </summary>
        public void ShowDialog()
        {
            var folderBrowserDialog = new FolderBrowserDialog
            {
                Description = @"Укажите каталог с CRL файлами",
                ShowNewFolderButton = false,
                RootFolder = Environment.SpecialFolder.CommonApplicationData
            };

            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.DirectoryPath = folderBrowserDialog.SelectedPath;
            this.CrlModels = GetCrlModels(this.DirectoryPath);
            this.CrlModels.CollectionChanged += this.CrlModels_CollectionChanged;
        }

        /// <summary>
        /// Сохраняет настройки в файл.
        /// </summary>
        public void Save()
        {
            String filePath = Path.Combine(ProgramDataPath, FileName);

            if (!Directory.Exists(ProgramDataPath))
            {
                Directory.CreateDirectory(ProgramDataPath);
            }

            var serializer = new SerializeHelper<SettingsModel>();
            serializer.Serialize(filePath, this);
            this.OnSettingsSaved();
        }

        /// <summary>
        /// Проверяет корректность заполненных настроек.
        /// </summary>
        /// <returns>true - настройки заполнены правильно, false - нет.</returns>
        public Boolean IsValid()
        {
            if (String.IsNullOrEmpty(this._directoryPath))
            {
                return false;
            }

            if (this._crlModels != null &&
                this._crlModels.Any(
                    crlModel => !String.IsNullOrEmpty(crlModel.FileName) && String.IsNullOrEmpty(crlModel.CrlName)))
            {
                return false;
            }

            return
                   !String.IsNullOrEmpty(this._serverAddress) &&
                   !String.IsNullOrEmpty(this._userName) &&
                   !String.IsNullOrEmpty(this._password) &&
                   !String.IsNullOrEmpty(this._exchange) &&
                   !String.IsNullOrEmpty(this._routingKey) &&
                   //!String.IsNullOrEmpty(this._queue) &&
                   Regex.IsMatch(
                       this._serverAddress,
                       @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|[A-Za-zА-ЯЁа-яё0-9_.]+") &&
                   this.CheckConnection();
        }

        /// <summary>
        /// Проверяет соединение с сервером по указанному адресу.
        /// </summary>
        /// <returns>true - если соединение с сервером установлено.</returns>
        public Boolean CheckConnection()
        {
            var rcm = new RabbitConnectionManager();
            rcm.ConfigureSettings(this._serverAddress, this._userName, this._password, this._exchange, this._routingKey, this._queue);
            rcm.CreateConnection();          
            return rcm.CheckConnection();
        }

        /// <summary>
        /// Скорректировать список файлов.
        /// </summary>
        /// <returns>Возвращает коллекцию новый файлов или null, если коллекция соответствует текущим файлам.</returns>
        public IEnumerable<CrlModel> CorrectCollection()
        {
            var actualCollection = GetCrlModels(this._directoryPath);
            for (Int32 i = this._crlModels.Count - 1; i >= 0; --i)
            {
                var crlModel = this._crlModels[i];
                if (actualCollection.All(c => c.FileName != crlModel.FileName))
                {
                    this._crlModels.Remove(crlModel);
                }
            }

            this.Save();

            if (actualCollection.Count > this._crlModels.Count)
            {
                for (Int32 i = actualCollection.Count - 1; i >= 0; --i)
                {
                    var crlModel = actualCollection[i];
                    if (this._crlModels.Any(c => c.FileName == crlModel.FileName))
                    {
                        actualCollection.Remove(crlModel);
                    }
                }

                return actualCollection;
            }

            return null;
        }

        /// <summary>
        /// Уведомляет подписчиков о событии сохранения настроек.
        /// </summary>
        protected virtual void OnSettingsSaved()
        {
            var handler = this.SettingsSaved;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Обрабатывает событие изменения списка файлов отзывов сертификатов.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void CrlModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyPropertyChanged("CrlModels");
        }
    }
}