using System;
using Iitrust.Utilities;
using Iitrust.Utilities.WPF;
using Infotecs.Pki.X509;
using DistinguishedName = Iitrust.Utilities.Enum.DistinguishedName;

namespace Iitrust.CRLPublication.Client.Models
{
    /// <summary>
    /// Модель списка отзывов сертификатов.
    /// </summary>
    public class CrlModel : Notifier, ISettingsModel
    {
        /// <summary>
        /// Имя файла в системе УЦ.
        /// </summary>
        private String _fileName;

        /// <summary>
        /// Имя CRL файла, используемое для передачи на сервер.
        /// </summary>
        private String _crlName;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CrlModel"/>.
        /// </summary>
        public CrlModel()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CrlModel"/>.
        /// </summary>
        /// <param name="fileFullPath">Полный путь к файлу.</param>
        /// <param name="fileName">Имя файла.</param>
        public CrlModel(String fileFullPath, String fileName)
        {
            this.FileName = fileName;
            var x509Crl = new X509Crl(fileFullPath);
            var commoName = x509Crl.Issuer.GetValue(DistinguishedName.CommonName);
            if (commoName.Contains("УЦ ИИТ"))
            {
                this.CrlName = String.Format("CA-IIT-{0}-YYYY.crl", commoName.Split()[2]);
            }
        }

        /// <summary>
        /// Событие возникающее после сохранения настроек.
        /// </summary>
        public event EventHandler SettingsSaved;

        /// <summary>
        /// Получает или задает имя файла в системе УЦ.
        /// </summary>
        public String FileName
        {
            get
            {
                return this._fileName;
            }

            set
            {
                this._fileName = value;
                this.NotifyPropertyChanged("FileName");
            }
        }

        /// <summary>
        /// Получает или задает имя CRL файла, которое используется для передачи на сервер.
        /// </summary>
        public String CrlName
        {
            get
            {
                return this._crlName;
            }

            set
            {
                this._crlName = value;
                this.NotifyPropertyChanged("CrlName");
            }
        }

        /// <summary>
        /// Сохраняет значения наименований.
        /// </summary>
        public void Save()
        {
            if (String.IsNullOrEmpty(this.CrlName))
            {
                this.CrlName = this.FileName;
            }

            this.OnSettingsSaved();
        }

        /// <summary>
        /// Проверяет валидность.
        /// </summary>
        /// <returns>Возвращает true.</returns>
        public Boolean IsValid()
        {
            return true;
        }

        /// <summary>
        /// Уведомляет подписчиков о сохранении настроек.
        /// </summary>
        protected virtual void OnSettingsSaved()
        {
            var handler = this.SettingsSaved;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}