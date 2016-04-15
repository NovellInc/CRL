using System;
using Newtonsoft.Json;

namespace MessageHandler.Extensions
{
    public static class JSONExtension
    {
        /// <summary>
        /// Десериализует строку из json в объект
        /// </summary>
        /// <param name="json">
        /// Строка для десериализации
        /// </param>
        /// <returns>
        /// Возвращает объект из десериализованной строки
        /// </returns>
        public static T Deserialize<T>(this String json) where T : Base64, new()
        {
            return JsonConvert.DeserializeObject<T>(json);
        } 
    }
}
