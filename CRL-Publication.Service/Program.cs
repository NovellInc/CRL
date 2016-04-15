using MessageHandler;
using MessageHandler.Extensions;
using MessageHandler.Properties;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing.Impl;

namespace Iitrust.CRLPublication.Service
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using NLog;
    using Services;
    using Topshelf;

    /// <summary>
    /// Класс консольного приложения.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Ведение журнала.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Точка входа для запускаемого сервиса.
        /// </summary>
        /// <param name="args">Массив передаваемых аргументов.</param>
        public static void Main(string[] args)
        {                        
            try
            {
                try
                {
                    ConfigurationHelper.CheckConfig();
                }
                catch (Exception)
                {
                    ConfigureService();
                }
                var host = HostFactory.New(x =>
                {
                    x.Service<PublicationService>(s =>
                    {
                        s.ConstructUsing(name => new PublicationService());
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });
                    x.RunAsLocalService();
                    x.UseNLog();
                    x.SetDescription(string.Format(
                        "Сервис обработки файлов отзывов сертификатов. Версия {0}",
                        Assembly.GetExecutingAssembly().GetName().Version.ToString()));
                    x.SetServiceName("Iitrust.CRLPublication.Service");
                    x.SetDisplayName("Iitrust CRL Files Processing Service");
                    x.StartAutomatically();
                    x.EnableServiceRecovery(rc =>
                    {
                        rc.RestartService(1);
                        rc.OnCrashOnly();
                    });
                });
                host.Run();                
            }
            catch (Exception exception)
            {
                Logger.Fatal("Возникла ошибка во время работы приложения: {0}", exception.Message);
                Console.Write("Для завершения работы приложения нажмите любую кнопку...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Настраивает службу.
        /// </summary>
        private static void ConfigureService()
        {
            ConfigureServerConnection();
            ConfigureFtp();
            ConfigureSmtp();
            ConfigureSmsc();
            ConfigureDb();
            ConfigureOtherSettings();
            ConfigurationHelper.SaveChanges();
            Console.Clear();
        }

        /// <summary>
        /// Настраивает СМС-центр.
        /// </summary>
        private static void ConfigureSmsc()
        {
            WriteCenterLine("SMS-центр");
            ConfigurationHelper.SmscLogin = ReadValue(
                ConfigurationHelper.SmscLogin,
                "Укажите имя учетной записи пользователя SMS-центра");
            ConfigurationHelper.SmscPassword =
                ReadPasswordValue("Укажите пароль учетной записи пользователя SMS-центра");
            ConfigurationHelper.SmscPhones = ReadValue(
                ConfigurationHelper.SmscPhones,
                "Укажите номера получателей SMS используя разделитель \";\", например, 79999999999",
                @"^([+]?[78]\d{10}[;]{1})+$").Trim(';');
            ConfigurationHelper.SmscTranslit =
                ReadValue(
                    ConfigurationHelper.SmscTranslit.ToString(),
                    "Переводить сообщение в транслит? [y/n]",
                    "[ynYN]").ToLower() == "y";
        }

        /// <summary>
        /// Настраивает адрес службы.
        /// </summary>
        private static void ConfigureServerConnection()
        {
            WriteCenterLine("Настройки подключеня к Rabbit серверу");
            ConfigurationHelper.ServerAddress = ReadValue(ConfigurationHelper.ServerAddress,
                                                          "Укажите адрес, по которому будет доступен Rabbit сервер",
                                                          @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|[A-Za-zА-ЯЁа-яё0-9_.]+");
            ConfigurationHelper.UserName = ReadValue(ConfigurationHelper.UserName, "Укажите имя пользователя");
            ConfigurationHelper.UserPassword = ReadPasswordValue("Укажите пароль");
            ConfigurationHelper.Exchange = ReadValue(ConfigurationHelper.Exchange, "Укажите точку обмена");
            ConfigurationHelper.RoutingKey = ReadValue(ConfigurationHelper.RoutingKey, "Укажите ключ маршрутизации");
            //ConfigurationHelper.Queue = ReadValue(ConfigurationHelper.Queue, "Укажите имя очереди");

            Console.WriteLine();
        }

        /// <summary>
        /// Настраивает подключение к БД.
        /// </summary>
        private static void ConfigureDb()
        {
            WriteCenterLine("MS SQL");

            ConfigurationHelper.DataSource = ReadValue(ConfigurationHelper.DataSource, "Укажите адрес SQL-сервера");
            ConfigurationHelper.InitialCatalog = ReadValue(ConfigurationHelper.InitialCatalog, "Укажите имя базы данных");
            ConfigurationHelper.IntegratedSecurity = ReadValue(
                ConfigurationHelper.IntegratedSecurity.ToString(),
                "Использовать Windows аутентификацию? [y/n]",
                @"[ynYN]")
                .ToLower() == "y";

            if (!ConfigurationHelper.IntegratedSecurity)
            {
                ConfigurationHelper.DbUser = ReadValue(ConfigurationHelper.DbUser, "Укажите пользователя SQL сервера");
                ConfigurationHelper.DbPassword = ReadPasswordValue("Укажите пароль пользователя SQL сервера");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Настраивает почту - SMTP.
        /// </summary>
        private static void ConfigureSmtp()
        {
            WriteCenterLine("SMTP");

            ConfigurationHelper.SmtpHost = ReadValue(ConfigurationHelper.SmtpHost, "Укажите адрес SMTP-сервера");
            ConfigurationHelper.SmtpPort = Convert.ToInt32(ReadValue(
                ConfigurationHelper.SmtpPort.ToString(),
                "Укажите порт SMTP-сервера",
                @"^\d+$"));
            ConfigurationHelper.SmtpUser = ReadValue(
                ConfigurationHelper.SmtpUser,
                "Укажите имя пользователя SMTP-сервера");
            ConfigurationHelper.SmtpPassword = ReadPasswordValue("Укажите пароль пользователя SMTP-сервера");
            ConfigurationHelper.SmtpSubject = ReadValue(
                ConfigurationHelper.SmtpSubject,
                "Укажите тему письма (необязательно), направляемого администратору при истечении даты следующего обновления списка отзыва сертификатов",
                "^.*$");
            ConfigurationHelper.SmtpFrom = ReadValue(
                ConfigurationHelper.SmtpFrom,
                "Укажите отправителя письма",
                @"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,6}");
            ConfigurationHelper.SmtpTo = ReadValue(
                ConfigurationHelper.SmtpTo == null ? String.Empty : String.Join(";", ConfigurationHelper.SmtpTo),
                "Укажите получателей письма используя разделитель \";\"",
                @"^(\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,6}[;]{1})+$").Trim(';').Split(';');

            Console.WriteLine();
        }

        /// <summary>
        /// Настраивает оставшиеся настройки.
        /// </summary>
        private static void ConfigureOtherSettings()
        {
            WriteCenterLine("Остальные настройки");

            ConfigurationHelper.TimeInterval = Convert.ToDouble(
                ReadValue(
                    ConfigurationHelper.TimeInterval.ToString(CultureInfo.InvariantCulture),
                    "Укажите интервал проверки (в минутах) даты следующего обновления списка отзыва сертификатов",
                    @"^\d+$"));

            ConfigurationHelper.UseProxy = ReadValue(
                ConfigurationHelper.UseProxy.ToString(),
                "Использовать прокси сервер? [y/n]",
                @"[ynYN]")
                .ToLower() == "y";

            if (ConfigurationHelper.UseProxy)
            {
                ConfigurationHelper.ProxyAddress = ReadValue(
                    ConfigurationHelper.ProxyAddress,
                    "Укажите URI прокси сервера",
                    @"http://((\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})|[A-Za-zА-ЯЁа-яё0-9_.]+):\d+/?");

                ConfigurationHelper.ProxyUser = ReadValue(
                    ConfigurationHelper.ProxyUser,
                    "Укажите пользователя прокси сервера, если его не указать, то будут использованы системные учетные данные приложения",
                    "^.*$");

                if (String.Empty != ConfigurationHelper.ProxyUser)
                {
                    ConfigurationHelper.ProxyPassword = ReadValue(
                        ConfigurationHelper.ProxyPassword,
                        "Укажите пароль пользователя прокси сервера");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Настраивает подключение к FTP.
        /// </summary>
        private static void ConfigureFtp()
        {
            WriteCenterLine("FTP");

            ConfigurationHelper.FtpAddress = ReadValue(ConfigurationHelper.FtpAddress, "Укажите адрес FTP-сервера (без указания порта)");
            ConfigurationHelper.FtpPort = ReadValue(ConfigurationHelper.FtpPort, "Укажите порт FTP-сервера", @"^\d+$");
            ConfigurationHelper.FtpUser = ReadValue(ConfigurationHelper.FtpUser, "Укажите имя пользователя FTP-сервера");
            ConfigurationHelper.FtpPassword = ReadPasswordValue("Введите пароль пользователя");

            Console.WriteLine();
        }

        /// <summary>
        /// Считывает значение из консоли, пока оно не удовлетворит условиям <param name="pattern"></param>.
        /// </summary>
        /// <param name="value">Текущее значение.</param>
        /// <param name="consoleMessage">Сообщение выводимое пользователю.</param>
        /// <param name="pattern">Шаблон для вводимого значения (по-умолчанию вводимое значение не должно быть пустым).</param>
        /// <returns>Возвращает считанное значение (знаки пробела вначале и вконце удаляются).</returns>
        private static String ReadValue(String value, String consoleMessage, String pattern = "^.+$")
        {
            String readValue;

            do
            {
                if (String.IsNullOrEmpty(value))
                {
                    Console.Write("{0}: ", consoleMessage);
                }
                else
                {
                    Console.Write("{0} (тек. знач. = {1}): ", consoleMessage, value);
                }

                readValue = Console.ReadLine();
                readValue = readValue == null ? String.Empty : readValue.Trim();

                if ((String.Empty == readValue && !String.IsNullOrEmpty(value)) || Regex.IsMatch(readValue, pattern))
                {
                    break;
                }
            }
            while (true);

            return String.IsNullOrEmpty(readValue) ? value : readValue;
        }

        /// <summary>
        /// Считывает пароль из консоли.
        /// </summary>
        /// <param name="consoleMessage">Сообщение выводимое пользотелю.</param>
        /// <returns>Возвращает считанное значение.</returns>
        private static String ReadPasswordValue(String consoleMessage)
        {
            StringBuilder readPassword;

            do
            {
                readPassword = new StringBuilder();
                Console.Write("{0}: ", consoleMessage);
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);

                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        readPassword.Append(key.KeyChar);
                        Console.Write("*");
                    }
                    else if (key.Key == ConsoleKey.Backspace && readPassword.Length > 0)
                    {
                        readPassword.Remove(readPassword.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                while (key.Key != ConsoleKey.Enter);
                Console.WriteLine();
            }
            while (readPassword.Length == 0);

            return readPassword.ToString();
        }

        /// <summary>
        /// Записывает заданное строковое значение по центру консоли.
        /// </summary>
        /// <param name="message">Строковое значение</param>
        private static void WriteCenterLine(String message)
        {
            Console.SetCursorPosition((Console.WindowWidth / 2) - (message.Length / 2), Console.CursorTop);
            Console.WriteLine("===== {0} =====", message);
        }
    }
}
