using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageHandler
{
    /// <summary>
    /// Описывает структуру передаваемого сообщения
    /// </summary>
    interface IBase64
    {
        /// <summary>
        /// Получает или задает название файла
        /// </summary>
        String FileName { get; set; }

        /// <summary>
        /// Получает или задает данные
        /// </summary>
        Byte[] Data { get; set; }
    }
}
