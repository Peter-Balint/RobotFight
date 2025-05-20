using RobotFight.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotFight_WPF.ViewModel
{
    public class GridImage : ViewModelBase
    {
        private int _content;
        public int Content{ 
            get { return _content; }
            set {  
                _content = value;
                OnPropertyChanged();
            }
        }
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

        public GridImage() {
            _content = 0;
            _rotation = 0;
        }
    }
}
