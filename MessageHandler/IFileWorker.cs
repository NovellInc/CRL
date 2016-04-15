using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageHandler
{
    /// <summary>
    /// Сохранение в файл, считываение из файла
    /// </summary>
    interface IFileWorker
    {
        /// <summary>
        /// Считывает данные из файла
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="fullName"></param>
        Boolean TryReadFromFile(string fullPath, string fullName);

        /// <summary>
        /// Сохраняет данные в файл
        /// </summary>
        Boolean TryWriteToFile();
    }
}
