using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace CRL_Publication.CRLUpdate
{
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
        /// Инициализирует класс ConfigurationHelper.
        /// </summary>
        static ConfigurationHelper()
        {
            Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

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
        }

        /// <summary>
        /// Сохраняет измененные настройки в конфигурационном файле.
        /// </summary>
        public static void SaveChanges()
        {
            Configuration.Save();
        }
    }
}
