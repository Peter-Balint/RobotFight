﻿using Android.App;
using Android.Content.PM;

using Avalonia;
using Avalonia.Android;

namespace RobotFight_Avalonia.Android;

[Activity(
    Label = "RobotFight",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/robot_orange",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
