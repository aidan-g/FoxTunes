﻿using FoxTunes.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LoadOutputStreamTask : BackgroundTask
    {
        public const string ID = "E3E23677-DE0A-4291-8416-BC4A91856037";

        public LoadOutputStreamTask(PlaylistItem playlistItem)
            : base(ID)
        {
            this.PlaylistItem = playlistItem;
        }

        public PlaylistItem PlaylistItem { get; private set; }

        public IOutput Output { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            this.Name = "Buffering";
            this.Description = new FileInfo(this.PlaylistItem.FileName).Name;
            this.OutputStreamQueue.Enqueue(await this.Output.Load(this.PlaylistItem));
        }
    }
}
