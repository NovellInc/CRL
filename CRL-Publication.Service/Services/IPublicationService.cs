using System;
using System.ServiceModel;

namespace Iitrust.CRLPublication.Service.Services
{
    /// <summary>
    /// Определяет методы сервиса обработки файлов отзывов сертификатов.
    /// </summary>
    [ServiceContract]
    public interface IPublicationService
    {
        /// <summary>
        /// Проверяет соединение.
        /// </summary>
        /// <returns>Возвращает true\false.</returns>
        [OperationContract]
        Boolean CheckConnection();

        /// <summary>
        /// Обрабатывает файл отзывов сертификатов.
        /// </summary>
        /// <param name="crlFileName">Наименование CRL файла.</param>
        /// <param name="crl">Массив байт, представляющий файл отзывов сертификатов.</param>
        //[FaultContract(typeof(FaultException))]
        //[OperationContract]
        //void ProcessFile(String crlFileName, Byte[] crl);
    }
}