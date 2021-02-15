using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class PlaylistColumn : PersistableComponent, ISequenceableComponent
    {
        public const double WIDTH_SMALL = 100;

        public const double WIDTH_LARGE = 300;

        public PlaylistColumn()
        {

        }

        private int _Sequence { get; set; }

        public int Sequence
        {
            get
            {
                return this._Sequence;
            }
            set
            {
                this._Sequence = value;
                this.OnSequenceChanged();
            }
        }

        protected virtual void OnSequenceChanged()
        {
            if (this.SequenceChanged != null)
            {
                this.SequenceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Sequence");
        }

        public event EventHandler SequenceChanged;

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

        private PlaylistColumnType _Type { get; set; }

        public PlaylistColumnType Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
                this.OnTypeChanged();
            }
        }

        protected virtual void OnTypeChanged()
        {
            if (this.TypeChanged != null)
            {
                this.TypeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Type");
        }

        public event EventHandler TypeChanged;

        private string _Tag { get; set; }

        public string Tag
        {
            get
            {
                return this._Tag;
            }
            set
            {
                this._Tag = value;
                this.OnTagChanged();
            }
        }

        protected virtual void OnTagChanged()
        {
            if (this.TagChanged != null)
            {
                this.TagChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Tag");
        }

        public event EventHandler TagChanged;

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

        private string _Plugin { get; set; }

        public string Plugin
        {
            get
            {
                return this._Plugin;
            }
            set
            {
                this._Plugin = value;
                this.OnPluginChanged();
            }
        }

        protected virtual void OnPluginChanged()
        {
            if (this.PluginChanged != null)
            {
                this.PluginChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Plugin");
        }

        public event EventHandler PluginChanged;

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

        private double? _Width { get; set; }

        public double? Width
        {
            get
            {
                return this._Width;
            }
            set
            {
                this._Width = value;
                this.OnWidthChanged();
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        private bool _Enabled { get; set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                this.OnEnabledChanged();
            }
        }

        protected virtual void OnEnabledChanged()
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Enabled");
        }

        public event EventHandler EnabledChanged;
    }

    public enum PlaylistColumnType : byte
    {
        None = 0,
        Script = 1,
        Plugin = 2,
        Tag = 3
    }
}
