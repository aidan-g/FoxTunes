using System;
using System.Globalization;

namespace FoxTunes
{
    public class PlaylistMetaDataBinding : MetaDataBinding
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlistItem = value as PlaylistItem;
            if (playlistItem == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(this.Name))
            {
                return null;
            }
            if (playlistItem.MetaDatas != null)
            {
                lock (playlistItem.MetaDatas)
                {
                    foreach (var item in playlistItem.MetaDatas)
                    {
                        if (!string.Equals(item.Name, this.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        return base.Convert(item.Value, targetType, parameter, culture);
                    }
                }
            }
            return null;
        }
    }
}
