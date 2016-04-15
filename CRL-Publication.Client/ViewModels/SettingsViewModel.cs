using System.Windows.Input;
using Iitrust.CRLPublication.Client.Commands;
using Iitrust.CRLPublication.Client.Models;

namespace Iitrust.CRLPublication.Client.ViewModels
{
    /// <summary>
    /// Реализует модель представления для представления <see cref="SettingsViewModel"/>.
    /// </summary>
    public class SettingsViewModel
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SettingsViewModel"/>.
        /// </summary>
        /// <param name="settings">Модель настроек приложения.</param>
        public SettingsViewModel(SettingsModel settings)
        {
            this.Settings = settings;
            this.OpenDirectoryCommand = new OpenDirectoryCommand(this.Settings);
            this.SaveSettingsCommand = new SaveSettingsCommand(this.Settings);
            this.CheckConnectionCommand = new CheckConnectionCommand(this.Settings);
        }

        /// <summary>
        /// Получает или задает модель настроек приложения.
        /// </summary>
        public SettingsModel Settings { get; set; }

        /// <summary>
        /// Получает или задает команду для открытия каталога с файлами.
        /// </summary>
        public ICommand OpenDirectoryCommand { get; set; }

        /// <summary>
        /// Получает или задает команду для сохранения настроек.
        /// </summary>
        public ICommand SaveSettingsCommand { get; set; }

        /// <summary>
        /// Получает или задает команду для проверки соединения.
        /// </summary>
        public ICommand CheckConnectionCommand { get; set; }
    }
}