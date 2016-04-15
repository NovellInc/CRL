namespace Iitrust.CRLPublication.Client
{
    using System.Windows;
    using Views;

    /// <summary>
    /// Приложение WPF.
    /// </summary>
    public class App : Application
    {
        /// <summary>
        /// Главное окно приложения.
        /// </summary>
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="App"/>.
        /// </summary>
        public App()
        {
            this._mainWindow = new MainWindow();
        }

        /// <summary>
        /// Обрабатывает открытие нового экземпляра приложения.
        /// </summary>
        public void OnStartupNextInstance()
        {
            this._mainWindow.ShowBalloonTip("Клиент уже запущен");
        }

        /// <summary>
        /// Обработчик события запуска приложения.
        /// </summary>
        /// <param name="eventArgs">Аргументы, связанные с событием запуска приложения.</param>
        protected override void OnStartup(StartupEventArgs eventArgs)
        {
            base.OnStartup(eventArgs);
            this._mainWindow.Show();
        }
    }
}