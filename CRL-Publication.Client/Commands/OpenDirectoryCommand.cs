﻿using System;
using System.Windows.Input;
using Iitrust.CRLPublication.Client.Models;

namespace Iitrust.CRLPublication.Client.Commands
{
    /// <summary>
    /// Реализует команду открытия каталога.
    /// </summary>
    public class OpenDirectoryCommand : ICommand
    {
        /// <summary>
        /// Модель настроек приложения.
        /// </summary>
        private readonly SettingsModel _settingsModel;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OpenDirectoryCommand"/>.
        /// </summary>
        /// <param name="settingsModel">Модель настроек приложения.</param>
        public OpenDirectoryCommand(SettingsModel settingsModel)
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
        /// Выполняется при запуске команды.
        /// </summary>
        /// <param name="parameter">Данные используемые командой.</param>
        public void Execute(Object parameter)
        {
            this._settingsModel.ShowDialog();
        }
    }
}