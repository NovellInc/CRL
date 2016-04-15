using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRL_Publication.CRLUpdate
{
    using System;
    using System.Globalization;
    using System.Configuration;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using NLog;
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
                ConfigureService();
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
                    x.Service<UpdateService>(s =>
                    {
                        s.ConstructUsing(name => new UpdateService());
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });
                    x.RunAsLocalService();
                    x.UseNLog();
                    x.SetDescription(string.Format(
                        "Сервис обновления отзывов сертификатов. Версия {0}",
                        Assembly.GetExecutingAssembly().GetName().Version.ToString()));
                    x.SetServiceName("Iitrust.CRLUpdate");
                    x.SetDisplayName("Iitrust CRL Updating Service");
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
            ConfigurationHelper.SaveChanges();
            Console.Clear();
        }


        /// <summary>
        /// Настраивает подключение к Rabbit серверу.
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
