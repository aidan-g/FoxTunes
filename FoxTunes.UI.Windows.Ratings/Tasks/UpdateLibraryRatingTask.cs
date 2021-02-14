using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdateLibraryRatingTask : BackgroundTask
    {
        public const string ID = "0DA1A44E-0FEC-492B-9F57-92F402196792";

        public UpdateLibraryRatingTask(LibraryHierarchyNode libraryHierarchyNode, byte rating) : base(ID)
        {
            this.LibraryHierarchyNode = libraryHierarchyNode;
            this.Rating = rating;
        }

        public UpdateLibraryRatingTask(IEnumerable<LibraryItem> libraryItems, byte rating) : base(ID)
        {
            this.LibraryItems = libraryItems;
            this.Rating = rating;
        }

        public LibraryHierarchyNode LibraryHierarchyNode { get; private set; }

        public IEnumerable<LibraryItem> LibraryItems { get; private set; }

        public byte Rating { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            var libraryItems = default(IEnumerable<LibraryItem>);
            if (this.LibraryHierarchyNode != null)
            {
                //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
                libraryItems = this.LibraryHierarchyBrowser.GetItems(
                    this.LibraryHierarchyNode,
                    true
                ).ToArray();
            }
            else if (this.LibraryItems != null)
            {
                libraryItems = this.LibraryItems;
            }
            else
            {
                //Nothing to do.
                return;
            }
            foreach (var libraryItem in libraryItems)
            {
                lock (libraryItem.MetaDatas)
                {
                    var metaDataItem = libraryItem.MetaDatas.FirstOrDefault(
                        _metaDataItem => string.Equals(_metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase)
                    );
                    if (metaDataItem == null)
                    {
                        metaDataItem = new MetaDataItem(CommonStatistics.Rating, MetaDataItemType.Tag);
                        libraryItem.MetaDatas.Add(metaDataItem);
                    }
                    metaDataItem.Value = Convert.ToString(this.Rating);
                }
            }
            await this.MetaDataManager.Save(libraryItems, true, false, CommonStatistics.Rating).ConfigureAwait(false);
        }
    }
}
