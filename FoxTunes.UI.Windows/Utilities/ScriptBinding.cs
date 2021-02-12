using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace FoxTunes
{
    public abstract class ScriptBinding : Binding, INotifyPropertyChanged, IValueConverter
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        protected ScriptBinding()
        {
            this.Converter = this;
        }

        private IScriptingContext _ScriptingContext { get; set; }

        public IScriptingContext ScriptingContext
        {
            get
            {
                return this._ScriptingContext;
            }
            set
            {
                this._ScriptingContext = value;
                this.OnScriptingContextChanged();
            }
        }

        protected virtual void OnScriptingContextChanged()
        {
            if (this.ScriptingContextChanged != null)
            {
                this.ScriptingContextChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ScriptingContext");
        }

        public event EventHandler ScriptingContextChanged;

        private string _Script { get; set; }

        public string Script
        {
            get
            {
                return this._Script;
            }
            set
            {
                this._Script = value;
                this.OnScriptChanged();
            }
        }

        protected virtual void OnScriptChanged()
        {
            if (this.ScriptChanged != null)
            {
                this.ScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Script");
        }

        public event EventHandler ScriptChanged;

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
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
