using RobotFight.Model;
using RobotFight.Persistence;
using RobotFight_Avalonia.ViewModels;
using System;

namespace RobotFight_Avalonia.Views
{
    public static class DesignData
    {
        public static RobotFightViewModel ViewModel
        {
            get
            {
                var model = new RobotFightModel(new RobotFightFileAccess());
                // egy elindított játékot rakunk be a nézetmodellbe, így a tervezőfelületen sem csak üres cellák lesznek
                return new RobotFightViewModel(model);
            }
        }
    }
}
