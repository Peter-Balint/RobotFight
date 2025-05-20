using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RobotFight.Model
{
    public enum GameAction { MoveRight, MoveLeft, MoveForward, MoveBack, TurnLeft, TurnRight, UseLaser, Melee }
    public enum Orientation { Up, Right, Down, Left }
    public class Player
    {
        private int x, y;
        private int health;
        public readonly int BaseHealth = 3;
        private Orientation orientation;
        public List<GameAction> Actions { get; private set; }
        private int boardSize;

        public event EventHandler<EventArgs>? Hit;
        public event EventHandler<PlayerActionEventArgs>? Moved;

        public int X { get { return x; } }
        public int Y { get { return y; } }
        public int Health {
            get { return health; }
            set {
                if (value >= 0 && value <= 3)
                {
                    int healthBefore = health;
                    health = value;
                    if (healthBefore > health)
                    {
                        Hit?.Invoke(this, new EventArgs());
                    }
                }
            }
        }
        public Orientation Orientation { 
            get { return orientation; } 
            set { orientation = value; } 
        }
        public int BoardSize
        {
            get { return boardSize; }
            set
            {
                if (value == 4 || value == 6 || value == 8)
                    boardSize = value;
            }
        }
        
        public Player()
        {
            Health = BaseHealth;
            Actions = new List<GameAction>();
        }

        public void SetPosition(int x, int y) {
            if(x >= 0 && x < BoardSize && y >= 0 && y < BoardSize)
            {
                Moved?.Invoke(this, new PlayerActionEventArgs(this, x, y));
                this.x = x;
                this.y = y;
            }
        }

    }
}
