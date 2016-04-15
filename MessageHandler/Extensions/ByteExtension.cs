using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageHandler.Extensions
{
    /// <summary>
    /// Расширение класса Byte
    /// </summary>
    public static class ByteExtension
    {
        /// <summary>
        /// Преобразует массив байт в строку
        /// </summary>
        /// <param name="array"></param>
        /// <returns>
        /// Возвращает строку
        /// </returns>
        public static String ConvertToString(this byte[] array)
        {
            return Encoding.Default.GetString(array);
        }
    } 
}
