namespace Iitrust.CRLPublication.Service
{
    using System;
    using System.Configuration;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Net.Configuration;
    using System.Security.Cryptography;
    using System.ServiceModel.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using CRL_Publication.Db;

    /// <summary>
    /// Вспомогательный класс для взаимодействия с файлом конфигурации.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Файл конфигурации.
        /// </summary>
        private static readonly Configuration Configuration;

        /// <summary>
        /// Настройки SMTP.
        /// </summary>
        private static readonly SmtpSection SmtpSection;

        /// <summary>
        /// Настройки строки подключения к БД.
        /// </summary>
        private static readonly ConnectionStringSettings ConnectionStringSettings;

        /// <summary>
        /// Строка подключения EntityFramework.
        /// </summary>
        private static readonly EntityConnectionStringBuilder EntityConnectionString;

        /// <summary>
        /// Строка подключения SQL.
        /// </summary>
        private static readonly SqlConnectionStringBuilder SqlConnectionString;

        /// <summary>
        /// Инициализирует класс ConfigurationHelper.
        /// </summary>
        static ConfigurationHelper()
        {
            Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            SmtpSection = GetSmtpSection();
            ConnectionStringSettings =
                Configuration.ConnectionStrings.ConnectionStrings[new CrlModelContainer().GetType().Name];

            EntityConnectionString = new EntityConnectionStringBuilder(ConnectionStringSettings.ConnectionString);
            SqlConnectionString = new SqlConnectionStringBuilder(EntityConnectionString.ProviderConnectionString);
        }

        #region FTP

        /// <summary>
        /// Возвращает или задает адрес FTP сервера.
        /// </summary>
        public static String FtpAddress
        {
            get
            {
                return Configuration.AppSettings.Settings["ftpAddress"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["ftpAddress"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает логин FTP сервера.
        /// </summary>
        public static String FtpUser
        {
            get
            {
                return Configuration.AppSettings.Settings["ftpLogin"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["ftpLogin"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает пароль FTP сервера.
        /// </summary>
        public static String FtpPassword
        {
            get
            {
                return Configuration.AppSettings.Settings["ftpPassword"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["ftpPassword"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает порт FTP сервера.
        /// </summary>
        public static String FtpPort
        {
            get
            {
                return Configuration.AppSettings.Settings["ftpPort"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["ftpPort"].Value = value;
            }
        }

        #endregion FTP

        #region Other

        /// <summary>
        /// Возвращает или задает время проверки заданий БД. Если в настройках не указано, то возвращает 1 минуту.
        /// </summary>
        public static Double TimeInterval
        {
            get
            {
                String value = Configuration.AppSettings.Settings["timeInterval"].Value;
                return !Regex.IsMatch(value, @"\d+") ? 1 : Convert.ToDouble(value);
            }

            set
            {
                Configuration.AppSettings.Settings["timeInterval"].Value = value <= 0 ? String.Empty : value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Возвращает или задает признак использования прокси сервера.
        /// </summary>
        public static Boolean UseProxy
        {
            get
            {
                Boolean useProxy;
                Boolean.TryParse(Configuration.AppSettings.Settings["useProxy"].Value, out useProxy);
                return useProxy;
            }

            set
            {
                Configuration.AppSettings.Settings["useProxy"].Value = value.ToString();
            }
        }

        /// <summary>
        /// Возвращает или задает адрес прокси сервера.
        /// </summary>
        public static String ProxyAddress
        {
            get
            {
                return Configuration.AppSettings.Settings["proxyAddress"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["proxyAddress"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает пользователя прокси сервера.
        /// </summary>
        public static String ProxyUser
        {
            get
            {
                return Configuration.AppSettings.Settings["proxyUser"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["proxyUser"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает пароль пользователя прокси сервера.
        /// </summary>
        public static String ProxyPassword
        {
            get
            {
                return Configuration.AppSettings.Settings["proxyPassword"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["proxyPassword"].Value = value;
            }
        }

        #endregion Other

        #region SMTP

        /// <summary>
        /// Возвращает или задает получателей почты.
        /// </summary>
        public static String[] SmtpTo
        {
            get
            {
                return String.IsNullOrEmpty(Configuration.AppSettings.Settings["smtpTo"].Value)
                    ? null
                    : Configuration.AppSettings.Settings["smtpTo"].Value.Split(';');
            }

            set
            {
                Configuration.AppSettings.Settings["smtpTo"].Value = String.Join(";", value);
            }
        }

        /// <summary>
        /// Возвращает или задает "тему" сообщения.
        /// </summary>
        public static String SmtpSubject
        {
            get
            {
                return Configuration.AppSettings.Settings["smtpSubject"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["smtpSubject"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает значение отправителя сообщений.
        /// </summary>
        public static String SmtpFrom
        {
            get
            {
                return SmtpSection.From;
            }

            set
            {
                SmtpSection.From = value;
            }
        }

        /// <summary>
        /// Возвращает или задает имя SMTP-сервера.
        /// </summary>
        public static String SmtpHost
        {
            get
            {
                return SmtpSection.Network.Host;
            }

            set
            {
                SmtpSection.Network.Host = value;
            }
        }

        /// <summary>
        /// Возвращает или задает порт SMTP-сервера.
        /// </summary>
        public static Int32 SmtpPort
        {
            get
            {
                return SmtpSection.Network.Port;
            }

            set
            {
                SmtpSection.Network.Port = value;
            }
        }

        /// <summary>
        /// Возвращает или задает имя пользователя SMTP-сервера.
        /// </summary>
        public static String SmtpUser
        {
            get
            {
                return SmtpSection.Network.UserName;
            }

            set
            {
                SmtpSection.Network.UserName = value;
            }
        }

        /// <summary>
        /// Возвращает или задает пароль пользователя SMTP-сервера.
        /// </summary>
        public static String SmtpPassword
        {
            get
            {
                return SmtpSection.Network.Password;
            }

            set
            {
                SmtpSection.Network.Password = value;
            }
        }

        #endregion SMTP

        #region DB

        /// <summary>
        /// Возвращает или задает адрес SQL сервера.
        /// </summary>
        public static String DataSource
        {
            get
            {
                return SqlConnectionString.DataSource;
            }

            set
            {
                SqlConnectionString.DataSource = value;
                EntityConnectionString.ProviderConnectionString = SqlConnectionString.ConnectionString;
                ConnectionStringSettings.ConnectionString = EntityConnectionString.ToString();
            }
        }

        /// <summary>
        /// Возвращает или задает имя базы данных.
        /// </summary>
        public static String InitialCatalog
        {
            get
            {
                return SqlConnectionString.InitialCatalog;
            }

            set
            {
                SqlConnectionString.InitialCatalog = value;
                EntityConnectionString.ProviderConnectionString = SqlConnectionString.ConnectionString;
                ConnectionStringSettings.ConnectionString = EntityConnectionString.ToString();
            }
        }

        /// <summary>
        /// Возвращает или задает логическое значение, которое определяет, используется Windows аутентификация или SQL сервера. 
        /// </summary>
        public static Boolean IntegratedSecurity
        {
            get
            {
                return SqlConnectionString.IntegratedSecurity;
            }

            set
            {
                SqlConnectionString.IntegratedSecurity = value;
                EntityConnectionString.ProviderConnectionString = SqlConnectionString.ConnectionString;
                ConnectionStringSettings.ConnectionString = EntityConnectionString.ToString();
            }
        }

        /// <summary>
        /// Возвращает или задает идентификатор пользователя SQL сервера.
        /// </summary>
        public static String DbUser
        {
            get
            {
                return SqlConnectionString.UserID;
            }

            set
            {
                SqlConnectionString.UserID = value;
                EntityConnectionString.ProviderConnectionString = SqlConnectionString.ConnectionString;
                ConnectionStringSettings.ConnectionString = EntityConnectionString.ToString();
            }
        }

        /// <summary>
        /// Возвращает или задает пароль для учетной записи SQL сервера.
        /// </summary>
        public static String DbPassword
        {
            get
            {
                return SqlConnectionString.Password;
            }

            set
            {
                SqlConnectionString.Password = value;
                EntityConnectionString.ProviderConnectionString = SqlConnectionString.ConnectionString;
                ConnectionStringSettings.ConnectionString = EntityConnectionString.ToString();
            }
        }

        #endregion DB

        #region Smsc

        /// <summary>
        /// Возвращает или задает учетную запись для сервиса отправки СМС.
        /// </summary>
        public static String SmscLogin
        {
            get
            {
                return Configuration.AppSettings.Settings["smscLogin"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["smscLogin"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает пароль от учетной записи сервиса отправки СМС.
        /// </summary>
        public static String SmscPassword
        {
            get
            {
                return Configuration.AppSettings.Settings["smscPassword"].Value;
            }

            set
            {
                using (var cryptography = MD5.Create())
                {
                    Byte[] dataHash = cryptography.ComputeHash(Encoding.UTF8.GetBytes(value));
                    var stringBuilder = new StringBuilder();
                    foreach (byte data in dataHash)
                    {
                        stringBuilder.Append(data.ToString("x2"));
                    }

                    Configuration.AppSettings.Settings["smscPassword"].Value = stringBuilder.ToString().ToLower();
                }
            }
        }

        /// <summary>
        /// Возвращает или задает номера телефонов для СМС рассылки.
        /// </summary>
        public static String SmscPhones
        {
            get
            {
                return Configuration.AppSettings.Settings["smscPhones"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["smscPhones"].Value = value;
            }
        }

        /// <summary>
        /// Возвращает или задает признак того, что сообщение необходимо перевести в транслит.
        /// </summary>
        public static Boolean SmscTranslit
        {
            get
            {
                Boolean useTranslit;
                Boolean.TryParse(Configuration.AppSettings.Settings["smscUseTranslit"].Value, out useTranslit);
                return useTranslit;
            }

            set
            {
                Configuration.AppSettings.Settings["smscUseTranslit"].Value = value.ToString();
            }
        }

        #endregion Smsc

        #region Rabbit сервер

        /// <summary>
        /// Получает или задает адрес используемый службой.
        /// </summary>
        public static String ServerAddress
        {
            get
            {
                return Configuration.AppSettings.Settings["serverAddress"].Value;
            }

            set
            {
                Configuration.AppSettings.Settings["serverAddress"].Value = value;                
            }
        }

        /// <summary>
        /// Получает или задает имя пользователя.
        /// </summary>
        public static String UserName
        {
            get
            {
                return Configuration.AppSettings.Settings["userName"].Value;
            }
            set
            {
                Configuration.AppSettings.Settings["userName"].Value = value;
            }
        }

        /// <summary>
        /// Получает или задает пароль пользователя.
        /// </summary>
        public static String UserPassword
        {
            get
            {
                return Configuration.AppSettings.Settings["userPassword"].Value;
            }
            set
            {
                Configuration.AppSettings.Settings["userPassword"].Value = value;
            }
        }

        /// <summary>
        /// Получает или задает точку обмена.
        /// </summary>
        public static String Exchange
        {
            get
            {
                return Configuration.AppSettings.Settings["exchange"].Value;
            }
            set
            {
                Configuration.AppSettings.Settings["exchange"].Value = value;
            }
        }

        /// <summary>
        /// Получает или задает ключ маршрутизации.
        /// </summary>
        public static String RoutingKey
        {
            get
            {
                return Configuration.AppSettings.Settings["routingKey"].Value;
            }
            set
            {
                Configuration.AppSettings.Settings["routingKey"].Value = value;
            }
        }

        /// <summary>
        /// Получает или задает имя очереди.
        /// </summary>
        public static String Queue
        {
            get
            {
                return Configuration.AppSettings.Settings["queue"].Value;
            }
            set
            {
                Configuration.AppSettings.Settings["queue"].Value = value;
            }
        }

        #endregion

        /// <summary>
        /// Проверяет заполнение необходимых настроек.
        /// </summary>
        public static void CheckConfig()
        {
            if (String.IsNullOrEmpty(ServerAddress))
            {
                throw new ConfigurationErrorsException("Не указан адрес Rabbit сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(UserName))
            {
                throw new ConfigurationErrorsException("Не указано имя пользователя. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(UserPassword))
            {
                throw new ConfigurationErrorsException("Не указан пароль пользователя. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(Exchange))
            {
                throw new ConfigurationErrorsException("Не указана точка обмена. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(RoutingKey))
            {
                throw new ConfigurationErrorsException("Не указан ключ маршрутизации. Проверьте настройки.");
            }

            //if (String.IsNullOrEmpty(Queue))
            //{
            //    throw new ConfigurationErrorsException("Не указано имя очереди. Проверьте настройки.");
            //}

            if (String.IsNullOrEmpty(FtpAddress))
            {
                throw new ConfigurationErrorsException("Не указан адрес FTP сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(FtpUser))
            {
                throw new ConfigurationErrorsException("Не указан пользователь FTP сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(FtpPassword))
            {
                throw new ConfigurationErrorsException("Не указан пароль пользователя FTP сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmtpHost))
            {
                throw new ConfigurationErrorsException("Не указан адрес SMTP сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmtpPort.ToString()))
            {
                throw new ConfigurationErrorsException("Не указан порт SMTP сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmtpUser))
            {
                throw new ConfigurationErrorsException("Не указано имя пользователя SMTP сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmtpPassword))
            {
                throw new ConfigurationErrorsException("Не указан пароль пользователя SMTP сервера. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmtpFrom))
            {
                throw new ConfigurationErrorsException("Не указан отправитель писем. Проверьте настройки.");
            }

            if (SmtpTo == null || SmtpTo.Length == 0)
            {
                throw new ConfigurationErrorsException("Не указан ни один получатель почты. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(DataSource))
            {
                throw new ConfigurationErrorsException("Не указан адрес SQL сервера. Проверьте настройки.");
            }

            if (string.IsNullOrEmpty(InitialCatalog))
            {
                throw new ConfigurationErrorsException("Не указано имя базы данных. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmscLogin))
            {
                throw new ConfigurationErrorsException("Не указана учетная запись для сервиса отправки СМС. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmscPassword))
            {
                throw new ConfigurationErrorsException("Не указана пароль учетной записи для сервиса отправки СМС. Проверьте настройки.");
            }

            if (String.IsNullOrEmpty(SmscPhones))
            {
                throw new ConfigurationErrorsException("Не указан ни один телефон для отправки СМС. Проверьте настройки.");
            }
        }

        /// <summary>
        /// Сохраняет измененные настройки в конфигурационном файле.
        /// </summary>
        public static void SaveChanges()
        {
            Configuration.Save();
        }

        /// <summary>
        /// Получает раздел SMTP в файле конфигурации.
        /// </summary>
        /// <returns>Раздел SMTP.</returns>
        private static SmtpSection GetSmtpSection()
        {
            var systemNetSection = Configuration.SectionGroups["system.net"];
            if (systemNetSection == null)
            {
                systemNetSection = new NetSectionGroup();
                Configuration.SectionGroups.Add("system.net", systemNetSection);
            }

            var mailSection = systemNetSection.SectionGroups["mailSettings"];
            if (mailSection == null)
            {
                mailSection = new MailSettingsSectionGroup();
                systemNetSection.SectionGroups.Add("mailSettings", mailSection);
            }

            if (mailSection.Sections["smtp"] == null)
            {
                mailSection.Sections.Add("smtp", new SmtpSection());
            }

            return (SmtpSection)mailSection.Sections["smtp"];
        }
    }
}