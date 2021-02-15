using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdatePlaylistPlayCountTask : BackgroundTask
    {
        public const string ID = "3489F57E-0C2D-48EC-AD28-A5C1F9167778";

        public UpdatePlaylistPlayCountTask(PlaylistItem playlistItem) : base(ID)
        {
            this.PlaylistItem = playlistItem;
        }

        public PlaylistItem PlaylistItem { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.Configuration = core.Components.Configuration;
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            lock (this.PlaylistItem.MetaDatas)
            {
                var metaDatas = this.PlaylistItem.MetaDatas.ToDictionary(
                    metaDataItem => metaDataItem.Name,
                    StringComparer.OrdinalIgnoreCase
                );
                this.UpdatePlayCount(metaDatas);
                this.UpdateLastPlayed(metaDatas);
            }
            var write = MetaDataBehaviourConfiguration.GetWriteBehaviour(
                this.Write.Value
            ).HasFlag(WriteBehaviour.Statistics);
            await this.MetaDataManager.Save(
                new[] { this.PlaylistItem },
                write,
                false,
                CommonStatistics.PlayCount,
                CommonStatistics.LastPlayed
            ).ConfigureAwait(false);
        }

        protected virtual void UpdatePlayCount(IDictionary<string, MetaDataItem> metaDatas)
        {
            var metaDataItem = default(MetaDataItem);
            if (!metaDatas.TryGetValue(CommonStatistics.PlayCount, out metaDataItem))
            {
                metaDataItem = new MetaDataItem(CommonStatistics.PlayCount, MetaDataItemType.Tag);
                this.PlaylistItem.MetaDatas.Add(metaDataItem);
            }
            var playCount = default(int);
            if (!string.IsNullOrEmpty(metaDataItem.Value) && int.TryParse(metaDataItem.Value, out playCount))
            {
                metaDataItem.Value = Convert.ToString(playCount + 1);
            }
            else
            {
                metaDataItem.Value = "1";
            }
        }

        protected virtual void UpdateLastPlayed(IDictionary<string, MetaDataItem> metaDatas)
        {
            var metaDataItem = default(MetaDataItem);
            if (!metaDatas.TryGetValue(CommonStatistics.LastPlayed, out metaDataItem))
            {
                metaDataItem = new MetaDataItem(CommonStatistics.LastPlayed, MetaDataItemType.Tag);
                this.PlaylistItem.MetaDatas.Add(metaDataItem);
            }
            //TODO: I can't work out what the standard is for this value.
            //TODO: I've seen DateTime.ToFileTime() but that seems a little too windows-ish.
            //TODO: Using yyyy/MM/dd HH:mm:ss for now.
            metaDataItem.Value = DateTimeHelper.ToString(DateTime.UtcNow);
        }
    }
}
