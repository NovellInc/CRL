using System;
using System.Windows;
using System.Windows.Input;
using Iitrust.CRLPublication.Client.Models;

namespace Iitrust.CRLPublication.Client.Commands
{
    /// <summary>
    /// Реализует команду проверки соединения с сервером.
    /// </summary>
    public class CheckConnectionCommand : ICommand
    {
        /// <summary>
        /// Модель настроек приложения.
        /// </summary>
        private readonly SettingsModel _settingsModel;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CheckConnectionCommand"/>.
        /// </summary>
        /// <param name="settingsModel">Модель настроек приложения</param>
        public CheckConnectionCommand(SettingsModel settingsModel)
        {
            this._settingsModel = settingsModel;
        }

        /// <summary>
        /// Событие, которое происходит при возникновении изменений, влияющих на то, должна ли выполняться данная команда.
        /// </summary>
        public event EventHandler CanExecuteChanged = delegate { };

        /// <summary>
        /// Может ли команда выполниться в текущем состоянии.
        /// </summary>
        /// <param name="parameter">Данные используемые командой.</param>
        /// <returns>Возвращает true - команду можно использовать всегда.</returns>
        public Boolean CanExecute(Object parameter)
        {
            return true;
        }

        /// <summary>
        /// Выполняетя при запуске команды.
        /// </summary>
        /// <param name="parameter">Данные используемые командой.</param>
        public void Execute(Object parameter)
        {
            Boolean result = this._settingsModel.CheckConnection();
            MessageBox.Show(
                String.Format("Соединение с сервером {0}установлено.", result ? String.Empty : "не "),
                "Проверка соединения",
                MessageBoxButton.OK,
                result ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
    }
}