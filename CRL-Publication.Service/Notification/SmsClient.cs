namespace Iitrust.CRLPublication.Service.Notification
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Реализует клиента для отправки SMS через SMS-центр.
    /// </summary>
    public static class SmsClient
    {
        /// <summary>
        /// Кодировка сообщения (windows-1251 или koi8-r), по умолчанию используется utf-8
        /// </summary>
        private const String SmscCharset = "utf-8";

        /// <summary>
        /// Адрес сервиса.
        /// </summary>
        private static String _serviceUri;

        /// <summary>
        /// Инициализирует статические переменные класса.
        /// </summary>
        static SmsClient()
        {
            _serviceUri = String.Format(
                "https://smsc.ru/sys/send.php?login={0}&psw={1}&phones={2}&charset={3}",
                ConfigurationHelper.SmscLogin,
                ConfigurationHelper.SmscPassword,
                ConfigurationHelper.SmscPhones,
                SmscCharset);
        }

        /// <summary>
        /// Отправляет СМС сообщение (<param name="message"></param>) на указанный список телефонов.
        /// </summary>
        /// <param name="message">Отправляемое сообщение.</param>
        public static void Send(String message)
        {
            _serviceUri += String.Format(
                "&mes={0}{1}",
                message,
                ConfigurationHelper.SmscTranslit ? "&translit=1" : String.Empty);
            String ret;
            Int32 i = 0;

            do
            {
                if (i > 0)
                {
                    Thread.Sleep(3000);
                }

                if (i == 2)
                {
                    ////Переключение на резервный сервер.
                    _serviceUri = _serviceUri.Replace("://smsc.ru/", "://www2.smsc.ru/");
                }

                var request = (HttpWebRequest)WebRequest.Create(_serviceUri);
                request.Credentials = CredentialCache.DefaultCredentials;

                if (ConfigurationHelper.UseProxy)
                {
                    request.Proxy = new WebProxy(new Uri(ConfigurationHelper.ProxyAddress))
                    {
                        Credentials =
                            String.Empty != ConfigurationHelper.ProxyUser
                                ? new NetworkCredential(ConfigurationHelper.ProxyUser, ConfigurationHelper.ProxyPassword)
                                : CredentialCache.DefaultCredentials
                    };
                }

                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    var sr = new StreamReader(response.GetResponseStream());
                    ret = sr.ReadToEnd();
                }
                catch (WebException)
                {
                    ret = String.Empty;
                }
            }
            while (ret == String.Empty && ++i < 4);
        }
    }
}