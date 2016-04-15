namespace Iitrust.CRLPublication.Client.Views
{
    using System.Windows;

    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SettingsWindow"/>.
        /// </summary>
        public SettingsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Событие нажатия на кнопку.
        /// </summary>
        /// <param name="sender">Инициатор события.</param>
        /// <param name="e">Информация связанная с событием.</param>
        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}