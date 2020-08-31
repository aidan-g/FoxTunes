using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Lyrics : ViewModelBase
    {
        public static readonly string PADDING = string.Join(string.Empty, Enumerable.Repeat(Environment.NewLine, 10));

        public IPlaybackManager PlaybackManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private bool _HasData { get; set; }

        public bool HasData
        {
            get
            {
                return this._HasData;
            }
            set
            {
                this._HasData = value;
                this.OnHasDataChanged();
            }
        }

        protected virtual void OnHasDataChanged()
        {
            if (this.HasDataChanged != null)
            {
                this.HasDataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasData");
        }

        public event EventHandler HasDataChanged;

        private bool _AutoScroll { get; set; }

        public bool AutoScroll
        {
            get
            {
                return this._AutoScroll;
            }
            set
            {
                this._AutoScroll = value;
                this.OnAutoScrollChanged();
            }
        }

        protected virtual void OnAutoScrollChanged()
        {
            if (this.AutoScroll)
            {
                PlaybackStateNotifier.Notify += this.OnNotify;
            }
            else
            {
                PlaybackStateNotifier.Notify -= this.OnNotify;
            }
            if (this.AutoScrollChanged != null)
            {
                this.AutoScrollChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AutoScroll");
        }

        public event EventHandler AutoScrollChanged;

        private string _Data { get; set; }

        public string Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.OnDataChanged();
            }
        }

        protected virtual void OnDataChanged()
        {
            if (this.DataChanged != null)
            {
                this.DataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Data");
        }

        public event EventHandler DataChanged;

        public long Position
        {
            get
            {
                if (!this.AutoScroll)
                {
                    return 0;
                }
                if (this.PlaybackManager == null)
                {
                    return 0;
                }
                var outputStream = this.PlaybackManager.CurrentStream;
                if (outputStream == null)
                {
                    return 0;
                }
                return outputStream.Position;
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged;

        public long Length
        {
            get
            {
                if (!this.AutoScroll)
                {
                    return 0;
                }
                if (this.PlaybackManager == null)
                {
                    return 0;
                }
                var outputStream = this.PlaybackManager.CurrentStream;
                if (outputStream == null)
                {
                    return 0;
                }
                return outputStream.Length;
            }
        }


        protected virtual void OnLengthChanged()
        {
            if (this.LengthChanged != null)
            {
                this.LengthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Length");
        }

        public event EventHandler LengthChanged;

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = this.Core.Managers.Playback;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
                    this.Dispatch(this.Refresh);
                }
                else
                {
                    this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
                    if (this.HasData)
                    {
                        var task = Windows.Invoke(() =>
                        {
                            this.HasData = false;
                            this.Data = null;
                        });
                    }
                }
            });
            this.Configuration.GetElement<BooleanConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_SCROLL
            ).ConnectValue(value => this.AutoScroll = value);
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    return this.Refresh(names);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnNotify(object sender, EventArgs e)
        {
            if (!this.HasData)
            {
                return;
            }
            this.OnPositionChanged();
        }

        protected virtual Task Refresh(IEnumerable<string> names)
        {
            if (names != null && names.Any())
            {
                if (!names.Contains(CommonMetaData.Lyrics, true))
                {
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            return this.Refresh();
        }

        protected virtual Task Refresh()
        {
            var data = default(string);
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                var metaDataItem = default(MetaDataItem);
                lock (outputStream.PlaylistItem.MetaDatas)
                {
                    metaDataItem = outputStream.PlaylistItem.MetaDatas.FirstOrDefault(
                        element => string.Equals(element.Name, CommonMetaData.Lyrics, StringComparison.OrdinalIgnoreCase)
                    );
                }
                if (metaDataItem != null)
                {
                    data = metaDataItem.Value;
                }
            }
            return Windows.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(data))
                {
                    this.Data = data;
                    this.HasData = true;
                }
                else
                {
                    this.HasData = false;
                    this.Data = null;
                }
                this.OnPositionChanged();
                this.OnLengthChanged();
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Lyrics();
        }

        protected override void OnDisposing()
        {
            PlaybackStateNotifier.Notify -= this.OnNotify;
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            base.OnDisposing();
        }
    }
}
