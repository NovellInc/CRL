using System.Windows.Input;
using Iitrust.CRLPublication.Client.Commands;
using Iitrust.CRLPublication.Client.Models;

namespace Iitrust.CRLPublication.Client.ViewModels
{
    /// <summary>
    /// Реализует модель представления для представления <see cref="FileCreatedViewModel"/>.
    /// </summary>
    public class FileCreatedViewModel
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SettingsViewModel"/>.
        /// </summary>
        /// <param name="crlModel">Модель файла списка отзывов сертификатов.</param>
        public FileCreatedViewModel(CrlModel crlModel)
        {
            this.SaveSettingsCommand = new SaveSettingsCommand(crlModel);
            this.Crl = crlModel;
        }

        /// <summary>
        /// Получает или задает команду для сохранения настроек.
        /// </summary>
        public ICommand SaveSettingsCommand { get; set; }

        /// <summary>
        /// Получает или задает модель файла списка отзывов сертификатов.
        /// </summary>
        public CrlModel Crl { get; set; }
    }
}