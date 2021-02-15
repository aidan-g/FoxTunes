using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace FoxTunes
{
    public abstract class MetaDataBinding : Binding, INotifyPropertyChanged, IValueConverter
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        protected MetaDataBinding()
        {
            this.Converter = this;
            this.Formatter = new Lazy<FormatterFactory.FormatterBase>(() => FormatterFactory.Create(this.Format));
        }

        public Lazy<FormatterFactory.FormatterBase> Formatter { get; private set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged;

        private string _Format { get; set; }

        public string Format
        {
            get
            {
                return this._Format;
            }
            set
            {
                this._Format = value;
                this.OnFormatChanged();
            }
        }

        protected virtual void OnFormatChanged()
        {
            if (this.FormatChanged != null)
            {
                this.FormatChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Format");
        }

        public event EventHandler FormatChanged;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Formatter.Value.GetValue(value);
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
