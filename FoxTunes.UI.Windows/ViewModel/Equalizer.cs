using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Equalizer : ViewModelBase
    {
        const string PRESET = "Preset";

        public Equalizer()
        {
            Windows.EqualizerWindowCreated += this.OnEqualizerWindowCreated;
            Windows.EqualizerWindowClosed += this.OnEqualizerWindowClosed;
            this.SelectedPreset = PRESET;
        }

        public bool Available
        {
            get
            {
                if (this.Effects == null || this.Effects.Equalizer == null || !this.Effects.Equalizer.Available)
                {
                    return false;
                }
                return true;
            }
        }

        protected virtual void OnAvailableChanged()
        {
            if (this.AvailableChanged != null)
            {
                this.AvailableChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Available");
        }

        public event EventHandler AvailableChanged;

        public bool Enabled
        {
            get
            {
                if (this.Effects == null || this.Effects.Equalizer == null)
                {
                    return false;
                }
                return this.Effects.Equalizer.Enabled;
            }
            set
            {
                if (this.Effects == null || this.Effects.Equalizer == null)
                {
                    return;
                }
                this.Effects.Equalizer.Enabled = value;
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

        public IEnumerable<EqualizerBand> Bands { get; private set; }

        protected virtual void OnBandsChanged()
        {
            if (this.BandsChanged != null)
            {
                this.BandsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Bands");
        }

        public event EventHandler BandsChanged;

        public IEnumerable<string> Presets
        {
            get
            {
                if (this.Effects == null || this.Effects.Equalizer == null)
                {
                    return null;
                }
                return new[] { PRESET }.Concat(this.Effects.Equalizer.Presets);
            }
        }

        protected virtual void OnPresetsChanged()
        {
            if (this.PresetsChanged != null)
            {
                this.PresetsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Presets");
        }

        public event EventHandler PresetsChanged;

        private string _SelectedPreset { get; set; }

        public string SelectedPreset
        {
            get
            {
                return this._SelectedPreset;
            }
            set
            {
                this._SelectedPreset = value;
                this.OnSelectedPresetChanged();
            }
        }

        protected virtual void OnSelectedPresetChanged()
        {
            if (this.SelectedPresetChanged != null)
            {
                this.SelectedPresetChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedPreset");
        }

        public event EventHandler SelectedPresetChanged;

        public IOutputEffects Effects { get; private set; }

        protected override void OnCoreChanged()
        {
            if (this.Core == null)
            {
                return;
            }
            this.Effects = this.Core.Components.OutputEffects;
            if (this.Effects.Equalizer != null)
            {
                this.Effects.Equalizer.AvailableChanged += this.OnAvailableChanged;
                this.Effects.Equalizer.EnabledChanged += this.OnEnabledChanged;
            }
            this.Bands = new List<EqualizerBand>(this.GetBands());
            //TODO: Bad .Wait().
            this.Refresh().Wait();
            base.OnCoreChanged();
        }

        protected virtual void OnEqualizerWindowCreated(object sender, EventArgs e)
        {
            this.OnEqualizerVisibleChanged();
        }

        protected virtual void OnEqualizerWindowClosed(object sender, EventArgs e)
        {
            this.OnEqualizerVisibleChanged();
        }

        protected virtual void OnAvailableChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnEnabledChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                this.OnAvailableChanged();
                this.OnEnabledChanged();
                this.OnBandsChanged();
                this.OnPresetsChanged();
            });
        }

        public bool EqualizerVisible
        {
            get
            {
                return Windows.IsEqualizerWindowCreated;
            }
            set
            {
                if (value)
                {
                    Windows.EqualizerWindow.DataContext = this.Core;
                    Windows.EqualizerWindow.Show();
                }
                else if (Windows.IsEqualizerWindowCreated)
                {
                    Windows.EqualizerWindow.Close();
                }
            }
        }

        protected virtual void OnEqualizerVisibleChanged()
        {
            if (this.EqualizerVisibleChanged != null)
            {
                this.EqualizerVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("EqualizerVisible");
        }

        public event EventHandler EqualizerVisibleChanged;

        public ICommand ShowCommand
        {
            get
            {
                return new Command(() => this.EqualizerVisible = true);
            }
        }

        public ICommand HideCommand
        {
            get
            {
                return new Command(() => this.EqualizerVisible = false);
            }
        }

        public ICommand ToggleCommand
        {
            get
            {
                return new Command(() => this.EqualizerVisible = !this.EqualizerVisible);
            }
        }

        public ICommand LoadPresetCommand
        {
            get
            {
                return new Command(() => this.LoadPreset());
            }
        }

        public void LoadPreset()
        {
            if (this.Effects == null || this.Effects.Equalizer == null)
            {
                return;
            }
            if (string.Equals(this.SelectedPreset, PRESET, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            this.Effects.Equalizer.LoadPreset(this.SelectedPreset);
            this.SelectedPreset = PRESET;
        }

        protected virtual IEnumerable<EqualizerBand> GetBands()
        {
            if (this.Effects.Equalizer != null)
            {
                foreach (var band in this.Effects.Equalizer.Bands)
                {
                    yield return new EqualizerBand(band)
                    {
                        Core = this.Core
                    };
                }
            }
        }

        protected override void OnDisposing()
        {
            Windows.EqualizerWindowCreated -= this.OnEqualizerWindowCreated;
            Windows.EqualizerWindowClosed -= this.OnEqualizerWindowClosed;
            if (this.Effects != null && this.Effects.Equalizer != null)
            {
                this.Effects.Equalizer.AvailableChanged -= this.OnAvailableChanged;
                this.Effects.Equalizer.EnabledChanged -= this.OnEnabledChanged;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Equalizer();
        }
    }
}
