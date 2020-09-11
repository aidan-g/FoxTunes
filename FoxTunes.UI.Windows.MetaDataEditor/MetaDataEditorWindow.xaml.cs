using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for MetaDataEditorWindow.xaml
    /// </summary>
    public partial class MetaDataEditorWindow : WindowBase
    {
        const int STARTUP_LOCATION_OFFSET = 90;

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public MetaDataEditorWindow()
        {
            var instance = Active.OfType<MetaDataEditorWindow>().LastOrDefault();
            if (instance != null)
            {
                this.Left = instance.Left + STARTUP_LOCATION_OFFSET;
                this.Top = instance.Top + STARTUP_LOCATION_OFFSET;
                this.Width = instance.Width;
                this.Height = instance.Height;
            }
            else if (!global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Top;
                }
                this.Width = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds.Height;
            }
            else
            {
                this.Width = 400;
                this.Height = 800;
            }
            if (double.IsNaN(this.Left) || double.IsNaN(this.Top))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            this.InitializeComponent();
        }

        public override string Id
        {
            get
            {
                return "82C7DF3A-5DD4-463D-B53C-A38DC1B9FF69";
            }
        }

        public void Show(IFileData[] fileDatas)
        {
            switch (fileDatas.Length)
            {
                case 1:
                    var title = Path.GetFileNameWithoutExtension(fileDatas[0].FileName);
                    var metaDatas = fileDatas[0].MetaDatas;
                    if (metaDatas != null)
                    {
                        foreach (var metaData in metaDatas)
                        {
                            if (!string.Equals(metaData.Name, CommonMetaData.Title, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            title = metaData.Value;
                            break;
                        }
                    }
                    this.Title = string.Format("Tags - {0}", title);
                    break;
                default:
                    this.Title = string.Format("Tags - {0} items", fileDatas.Length);
                    break;
            }
            this.MetaDataEditor.Edit(fileDatas);
            this.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            global::FoxTunes.Properties.Settings.Default.MetaDataWindowBounds = this.RestoreBounds;
            global::FoxTunes.Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
