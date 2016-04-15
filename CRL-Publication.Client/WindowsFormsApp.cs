namespace Iitrust.CRLPublication.Client
{
    using System;
    using Microsoft.VisualBasic.ApplicationServices;

    /// <summary>
    /// Предоставляет информацию о текущем приложении.
    /// </summary>
    public class WindowsFormsApp : WindowsFormsApplicationBase
    {
        /// <summary>
        /// Приложение WPF.
        /// </summary>
        private App _wpfApp;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WindowsFormsApp"/>.
        /// </summary>
        public WindowsFormsApp()
        {
            this.IsSingleInstance = true;
        }

        /// <summary>
        /// Обработчик события запуска приложения.
        /// </summary>
        /// <param name="eventArgs">Данные, связанные с событием запуска приложения.</param>
        /// <returns>Возвращает false.</returns>
        protected override Boolean OnStartup(StartupEventArgs eventArgs)
        {
            this._wpfApp = new App();
            this._wpfApp.Run();

            return false;
        }

        /// <summary>
        /// Обработчик события запуска нового экземпляра приложения.
        /// </summary>
        /// <param name="eventArgs">Данные, связанные с событием запуска приложения.</param>
        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            this._wpfApp.OnStartupNextInstance();
            base.OnStartupNextInstance(eventArgs);
        }
    }
}