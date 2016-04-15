using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageHandler
{
    /// <summary>
    /// Класс описывает структуру хранения информации о полученном сообщении
    /// </summary>
    public class RecievedMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Имя файла
        /// </summary>
        private readonly String _crlFileName;

        /// <summary>
        /// Содержимое файла
        /// </summary>
        private readonly Byte[] _crl;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RecievedMessageEventArgs"/>
        /// </summary>
        /// <param name="crlFileName">Имя файла</param>
        /// <param name="crl">Содержимое файла</param>
        public RecievedMessageEventArgs(String crlFileName, Byte[] crl)
        {
            this._crlFileName = crlFileName;
            this._crl = crl;
        }

        /// <summary>
        /// Получает имя файла
        /// </summary>
        public String CrlFileName
        {
            get
            {
                return this._crlFileName;
            }
        }

        /// <summary>
        /// Получает содержимое файла
        /// </summary>
        public Byte[] Crl
        {
            get
            {
                return this._crl;
            }
        }
    }
}
