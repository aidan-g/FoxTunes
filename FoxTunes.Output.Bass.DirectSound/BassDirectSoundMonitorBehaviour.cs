﻿using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class BassDirectSoundMonitorBehaviour : BassDeviceMonitorBehaviour
    {
        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.MODE_ELEMENT
            ).ConnectValue<string>(value => this.Enabled = string.Equals(value, BassDirectSoundStreamOutputConfiguration.MODE_DS_OPTION, StringComparison.OrdinalIgnoreCase));
            base.InitializeComponent(core);
        }

        protected override bool RestartRequired(DataFlow? flow, Role? role)
        {
            if (!BassDirectSoundDevice.IsDefaultDevice)
            {
                return false;
            }
            return base.RestartRequired(flow, role);
        }
    }
}