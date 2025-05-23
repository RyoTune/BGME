﻿using BGME.Framework.P3R.Template.Configuration;
using System.ComponentModel;

namespace BGME.Framework.P3R.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [DisplayName("Disable Victory Theme")]
    [DefaultValue(false)]
    public bool DisableVictoryBgm { get; set; } = false;

    [Category("SMT5V")]
    [DisplayName("BGM Volume Fix for Ryo Framework")]
    [DefaultValue(true)]
    public bool SMT5V_UseVolumeFix { get; set; } = true;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}