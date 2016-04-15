using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageHandler.Extensions;

namespace MessageHandler
{
    /// <summary>
    /// Описывает структуру данных файла
    /// </summary>
    public class Base64 : IBase64, IFileWorker
    {

        #region Поля данных

        /// <summary>
        /// Получает или задает название файла
        /// </summary>
        public String FileName { get; set; }

        /// <summary>
        /// Получает или задает данные
        /// </summary>
        public Byte[] Data { get; set; }

        #endregion


        /// <summary>
        /// Инициализирует объект класса с задаваемыми параметрами
        /// </summary>
        public Base64(String filename, Byte[] data)
        {
            this.FileName = filename;
            this.Data = data;
        }

        /// <summary>
        /// Инициализирует объект класса
        /// </summary>
        public Base64()
        {
        }

        /// <summary>
        /// Считывает данные из файла
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool TryReadFromFile(string fullPath, string fileName)
        {
            try
            {
                this.Data = Convert.ToBase64String(File.ReadAllBytes(fullPath)).ConvertToBytes();
                this.FileName = fileName;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Запись в файл данных
        /// </summary>
        /// <returns></returns>
        public bool TryWriteToFile()
        {
            try
            {
                File.WriteAllBytes(this.FileName, Convert.FromBase64String(this.Data.ConvertToString()));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
