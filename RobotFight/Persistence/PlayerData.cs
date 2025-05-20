using RobotFight.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotFight.Persistence
{
    public class PlayerData
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Health { get; private set; }
        public int BoardSize {  get; private set; }
        public Orientation Orientation {  get; private set; }
        public PlayerData(int x, int y, int Health, int BoardSize, Orientation Orientation) 
        {
            this.X = x;
            this.Y = y;
            this.Health = Health;
            this.BoardSize = BoardSize;
            this.Orientation = Orientation;
        }
    }
}
