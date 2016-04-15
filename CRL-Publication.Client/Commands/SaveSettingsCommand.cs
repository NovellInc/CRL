using System;
using System.Windows.Input;
using Iitrust.CRLPublication.Client.Models;

namespace Iitrust.CRLPublication.Client.Commands
{
    /// <summary>
    /// Реализует команду сохранения настроек.
    /// </summary>
    public class SaveSettingsCommand : ICommand
    {
        /// <summary>
        /// Модель настроек приложения.
        /// </summary>
        private readonly ISettingsModel _settingsModel;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OpenDirectoryCommand"/>.
        /// </summary>
        /// <param name="settingsModel">Модель настроек приложения.</param>
        public SaveSettingsCommand(ISettingsModel settingsModel)
        {
            this._settingsModel = settingsModel;
            this._settingsModel.PropertyChanged += (sender, args) =>
            {
                this.CanExecuteChanged(null, null);
            };
        }

        /// <summary>
        /// Событие, которое происходит при возникновении изменений, влияющих на то, должна ли выполняться данная команда.
        /// </summary>
        public event EventHandler CanExecuteChanged = delegate { };

        /// <summary>
        /// Может ли команда выполниться в текущем состоянии.
        /// </summary>
        /// <param name="parameter">Данные используемые командой.</param>
        /// <returns>Возвращает true - если заполнены все поля настроек.</returns>
        public Boolean CanExecute(Object parameter)
        {
            return this._settingsModel.IsValid();
        }

        /// <summary>
        /// Выполняется при запуске команды.
        /// </summary>
        /// <param name="parameter">Данные используемые командой.</param>
        public void Execute(Object parameter)
        {
            this._settingsModel.Save();
        }
    }
}