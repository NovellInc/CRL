using System;
using System.ComponentModel;

namespace Iitrust.CRLPublication.Client.Models
{
    /// <summary>
    /// Описывает модель настроек приложения.
    /// </summary>
    public interface ISettingsModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие сохранение настроек.
        /// </summary>
        event EventHandler SettingsSaved;

        /// <summary>
        /// Сохраняет настройки в файл.
        /// </summary>
        void Save();

        /// <summary>
        /// Проверяет корректность заполненных настроек.
        /// </summary>
        /// <returns>true - настройки заполнены правильно, false - нет.</returns>
        Boolean IsValid();
    }
}