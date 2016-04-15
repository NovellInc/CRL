namespace Iitrust.CRLPublication.Client.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;
    using Models;
    using ViewModels;
    using MessageHandler;

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Значок в области уведомлений.
        /// </summary>
        private NotifyIcon _notifyIcon;
        
        /// <summary>
        /// Окно настроек приложения.
        /// </summary>
        private SettingsWindow _settingsWindow;

        /// <summary>
        /// Модель настроек.
        /// </summary>
        private SettingsModel _settingsModel;

        /// <summary>
        /// Контекстное меню значка в области уведомлений.
        /// </summary>
        private ContextMenu _contextMenu;

        /// <summary>
        /// Состояние обработки.
        /// </summary>
        private ProcessingStatus _processingStatus;

        /// <summary>
        /// Модель обработки файла.
        /// </summary>
        private FileProcessingModel _fileProcessing;

        /// <summary>
        /// Окно добавления нового файла.
        /// </summary>
        private List<FileCreatedWindow> _fileCreatedWindows = new List<FileCreatedWindow>();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.InitializeNotifyIcon();
            this.InitializeContextMenu();
            this.InitializeSettings();
        }

        /// <summary>
        /// Отображает всплывающую подсказку.
        /// </summary>
        /// <param name="message">Сообщение выводимое во всплывающей подсказке.</param>
        /// <param name="title">Заголовок всплывающей подсказки.</param>
        public void ShowBalloonTip(string message, string title = "Уведомление")
        {
            this._notifyIcon.ShowBalloonTip(
                5,
                title,
                message,
                ToolTipIcon.Info);
        }

        /// <summary>
        /// Инициализирует настройки приложения.
        /// </summary>
        private void InitializeSettings()
        {
            this._settingsModel = SettingsModel.Open() ?? new SettingsModel();
            this._settingsModel.SettingsSaved += (sender, args) =>
            {
                if (this._settingsModel.IsValid())
                {
                    this._contextMenu.MenuItems[0].Enabled = true;
                }
            };
        }

        /// <summary>
        /// Инициализирует контекстное меню.
        /// </summary>
        private void InitializeContextMenu()
        {
            this._contextMenu = new ContextMenu();
            this._contextMenu.MenuItems.Add(0, new MenuItem("&Старт", this.StartStopClick));
            this._contextMenu.MenuItems.Add(1, new MenuItem("&Настройки", this.SettingsClick));
            this._contextMenu.MenuItems.Add("-");
            this._contextMenu.MenuItems.Add(3, new MenuItem("&Выход", this.ExitClick));
            this._notifyIcon.ContextMenu = this._contextMenu;
        }

        /// <summary>
        /// Событие нажатия на кнопку Выход.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void ExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Событие нажатия на кнопку Настройки.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void SettingsClick(object sender, EventArgs e)
        {
            this._settingsWindow = new SettingsWindow
            {
                DataContext = new SettingsViewModel(this._settingsModel)
            };
            this._settingsWindow.ShowDialog();
        }

        /// <summary>
        /// Событие нажатия на кнопку Старт\Стоп.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void StartStopClick(object sender, EventArgs e)
        {
            if (this._processingStatus == ProcessingStatus.Stop)
            {
                if (this._fileProcessing == null)
                {
                    this._fileProcessing = new FileProcessingModel(this._settingsModel);
                    this._fileProcessing.Created += this.FileProcessing_Created;
                    this._fileProcessing.Changed += this.FileProcessing_Changed;
                    this._fileProcessing.Deleted += this.FileProcessing_Deleted;
                    this._fileProcessing.Renamed += this.FileProcessing_Renamed;
                }

                this._contextMenu.MenuItems[0].Text = @"&Стоп";
                this._fileProcessing.Start();
                this._processingStatus = ProcessingStatus.Start;
                this.ShowBalloonTip("Процесс запущен");
            }
            else
            {
                this.StopProcess();
            }
        }

        /// <summary>
        /// Обрабатывает событие возникновения ошибки связи со службой.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileProcessing_FaultExceptionOccurred(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ShowBalloonTip("Сервер не смог обработать отправленный файл");
            }));
        }

        /// <summary>
        /// Останавливает процесс обработки.
        /// </summary>
        /// <param name="message">Сообщение отображаемое во всплывающей подсказке (по-умолчанию: Процесс остановлен).</param>
        private void StopProcess(string message = "Процесс остановлен")
        {
            this._contextMenu.MenuItems[0].Text = @"&Старт";
            this._fileProcessing.Stop();
            this._processingStatus = ProcessingStatus.Stop;
            this.ShowBalloonTip(message);
        }

        /// <summary>
        /// Обработчик события изменения файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileProcessing_Changed(object sender, FileSystemEventArgs e)
        {
        }

        /// <summary>
        /// Обработчик события удаления файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileProcessing_Deleted(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var crlModel = this._settingsModel.CrlModels.First(c => c.FileName == e.Name);
                this._settingsModel.CrlModels.Remove(crlModel);
            }));
        }

        /// <summary>
        /// Обработчик события переименования файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileProcessing_Renamed(object sender, RenamedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                var crlModel = this._settingsModel.CrlModels.First(c => c.FileName == e.OldName);
                crlModel.FileName = e.Name;
            }));
        }

        /// <summary>
        /// Обработчик события создания файла в файловой системе.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void FileProcessing_Created(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.ShowBalloonTip(
                    "Обнаружен новый файл в системе. Этот файл будет обработан при следующем изменении или после перезапуска процесса.");

                var crlModel = new CrlModel(e.FullPath, e.Name);

                var fileCreatedWindow = new FileCreatedWindow
                {
                    DataContext = new FileCreatedViewModel(crlModel)
                };

                this._fileCreatedWindows.Add(fileCreatedWindow);

                fileCreatedWindow.Show();
                crlModel.SettingsSaved += (o, args) =>
                {
                    fileCreatedWindow.Close();
                    this._fileCreatedWindows.Remove(fileCreatedWindow);
                };

                this._settingsModel.CrlModels.Add(crlModel);
            }));
        }

        /// <summary>
        /// Инициализирует значок в области уведомлений.
        /// </summary>
        private void InitializeNotifyIcon()
        {
            this._notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Resource.certificate3
            };
        }

        /// <summary>
        /// Событие закрытия окна (приложения).
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (this._notifyIcon != null)
            {
                this._notifyIcon.Dispose();
                this._notifyIcon = null;
            }

            if (this._settingsWindow != null)
            {
                this._settingsWindow.Close();
                this._settingsWindow = null;
            }

            if (this._fileCreatedWindows != null && this._fileCreatedWindows.Count > 0)
            {
                this._fileCreatedWindows.ForEach(w => w.Close());
                this._fileCreatedWindows = null;
            }
        }

        /// <summary>
        /// Событие загрузки окна.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this._contextMenu.MenuItems[0].Enabled = this._settingsModel.IsValid();
            this._processingStatus = ProcessingStatus.Stop;

            this.ShowBalloonTip("Клиент запущен");
        }
    }
}