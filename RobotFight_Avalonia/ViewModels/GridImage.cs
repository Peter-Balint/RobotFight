using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotFight_Avalonia.ViewModels
{
    public class GridImage : ViewModelBase
    {
        
        private int _rotation;
        public int Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                OnPropertyChanged();
            }
        }

        public GridImage()
        {

            _rotation = 0;
            IsRobot1 = false;
            IsRobot2 = false;
        }

        private bool _isRobot1;
        public bool IsRobot1 {
            get { return _isRobot1;}
            set
            {
                _isRobot1 = value;
                OnPropertyChanged();
            } 
        }
        private bool _isRobot2;
        public bool IsRobot2
        {
            get { return _isRobot2; }
            set
            {
                _isRobot2 = value;
                OnPropertyChanged();
            }
        }
    }
}
