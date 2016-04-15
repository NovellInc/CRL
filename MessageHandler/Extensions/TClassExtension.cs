using System;
using Newtonsoft.Json;

namespace MessageHandler.Extensions
{
    public static class TClassExtension
    {
        /// <summary>
        /// Сериализует объект в строку json
        /// </summary>
        /// <param name="t">
        /// Объект для сериализации
        /// </param>
        /// <returns> 
        /// Возвращает сериализованную строку
        /// </returns>
        public static String Serialize<T>(this T t) where T : Base64, new()
        {
            return JsonConvert.SerializeObject(t);
        }
    }
}
