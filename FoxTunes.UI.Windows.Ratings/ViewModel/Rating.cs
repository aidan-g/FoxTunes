using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Rating : ViewModelBase
    {
        public static readonly DependencyProperty FileDataProperty = DependencyProperty.Register(
            "FileData",
            typeof(IFileData),
            typeof(Rating),
            new PropertyMetadata(new PropertyChangedCallback(OnFileDataChanged))
        );

        public static IFileData GetFileData(Rating source)
        {
            return (IFileData)source.GetValue(FileDataProperty);
        }

        public static void SetFileData(Rating source, IFileData value)
        {
            source.SetValue(FileDataProperty, value);
        }

        public static void OnFileDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var rating = sender as Rating;
            if (rating == null)
            {
                return;
            }
            rating.OnFileDataChanged();
        }

        public bool Star1
        {
            get
            {
                return this.Value >= 1;
            }
            set
            {
                if (value || this.Value > 1)
                {
                    this.SetValue(1);
                }
                else
                {
                    this.SetValue(0);
                }
            }
        }

        public bool Star2
        {
            get
            {
                return this.Value >= 2;
            }
            set
            {
                if (value || this.Value > 1)
                {
                    this.SetValue(2);
                }
                else
                {
                    this.SetValue(1);
                }
            }
        }

        public bool Star3
        {
            get
            {
                return this.Value >= 3;
            }
            set
            {
                if (value || this.Value > 2)
                {
                    this.SetValue(3);
                }
                else
                {
                    this.SetValue(2);
                }
            }
        }

        public bool Star4
        {
            get
            {
                return this.Value >= 4;
            }
            set
            {
                if (value || this.Value > 3)
                {
                    this.SetValue(4);
                }
                else
                {
                    this.SetValue(3);
                }
            }
        }

        public bool Star5
        {
            get
            {
                return this.Value >= 5;
            }
            set
            {
                if (value || this.Value > 4)
                {
                    this.SetValue(5);
                }
                else
                {
                    this.SetValue(4);
                }
            }
        }

        public byte Value
        {
            get
            {
                var value = default(byte);
                if (this.MetaDataItem != null && byte.TryParse(this.MetaDataItem.Value, out value))
                {
                    return value;
                }
                return 0;
            }
        }

        protected virtual void OnValueChanged(byte value)
        {
            if (this.ValueChanged == null)
            {
                return;
            }
            this.ValueChanged(this, new RatingEventArgs(this.FileData, value));
        }

        public event RatingEventHandler ValueChanged;

        protected virtual void SetValue(byte value)
        {
            this.OnValueChanged(value);
        }

        private MetaDataItem _MetaDataItem { get; set; }

        public MetaDataItem MetaDataItem
        {
            get
            {
                return this._MetaDataItem;
            }
            set
            {
                this.OnMetaDataItemChanging();
                this._MetaDataItem = value;
                this.OnMetaDataItemChanged();
            }
        }

        protected virtual void OnMetaDataItemChanging()
        {
            if (this.MetaDataItem != null)
            {
                this.MetaDataItem.ValueChanged -= this.OnMetaDataItemValueChanged;
            }
        }

        protected virtual void OnMetaDataItemChanged()
        {
            if (this.MetaDataItem != null)
            {
                this.MetaDataItem.ValueChanged += this.OnMetaDataItemValueChanged;
            }
            this.Refresh();
            if (this.MetaDataItemChanged != null)
            {
                this.MetaDataItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MetaDataItem");
        }

        public event EventHandler MetaDataItemChanged;

        protected virtual void OnMetaDataItemValueChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(new Action(this.Refresh));
        }

        public IFileData FileData
        {
            get
            {
                return GetFileData(this);
            }
            set
            {
                SetFileData(this, value);
            }
        }

        protected virtual void OnFileDataChanged()
        {
            this.Bind();
            if (this.FileDataChanged != null)
            {
                this.FileDataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FileData");
        }

        public event EventHandler FileDataChanged;

        protected virtual void Bind()
        {
            if (this.FileData == null)
            {
                this.MetaDataItem = null;
            }
            else
            {
                lock (this.FileData.MetaDatas)
                {
                    var metaDataItem = this.FileData.MetaDatas.FirstOrDefault(
                        _metaDataItem => string.Equals(_metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase)
                    );
                    if (metaDataItem == null)
                    {
                        metaDataItem = new MetaDataItem(CommonStatistics.Rating, MetaDataItemType.Tag);
                        this.FileData.MetaDatas.Add(metaDataItem);
                    }
                    this.MetaDataItem = metaDataItem;
                }
            }
        }

        protected virtual void Refresh()
        {
            for (var a = 1; a <= 5; a++)
            {
                this.OnPropertyChanged("Star" + a);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Rating();
        }
    }
}
