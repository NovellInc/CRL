using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageHandler.Extensions
{
    /// <summary>
    /// Расширение класса String
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Преобразует строку в массив байт
        /// </summary>
        /// <param name="str"></param>
        /// <returns>
        /// Возвращает массив байт
        /// </returns>
        public static byte[] ConvertToBytes(this String str)
        {
            return Encoding.Default.GetBytes(str);
        }
    }
}
