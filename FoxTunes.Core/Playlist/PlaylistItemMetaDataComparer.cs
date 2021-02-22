using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class PlaylistItemMetaDataComparer : BaseComponent, IComparer<PlaylistItem>
    {
        public PlaylistItemMetaDataComparer(string tag)
        {
            this.Tag = tag;
        }

        public string Tag { get; private set; }

        public int Compare(PlaylistItem playlistItem1, PlaylistItem playlistItem2)
        {
            var value1 = this.GetValue(playlistItem1);
            var value2 = this.GetValue(playlistItem2);
            return this.Compare(value1, value2);
        }

        protected virtual int Compare(string value1, string value2)
        {
            var numeric1 = default(float);
            var numeric2 = default(float);
            if (float.TryParse(value1, out numeric1) && float.TryParse(value2, out numeric2))
            {
                return numeric1.CompareTo(numeric2);
            }
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        protected virtual string GetValue(PlaylistItem playlistItem)
        {
            lock (playlistItem.MetaDatas)
            {
                var metaDataItem = playlistItem.MetaDatas.FirstOrDefault(
                    _metaDataItem => string.Equals(_metaDataItem.Name, this.Tag)
                );
                if (metaDataItem != null)
                {
                    return metaDataItem.Value;
                }
            }
            return null;
        }
    }
}
