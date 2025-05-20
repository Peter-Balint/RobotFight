namespace RobotFight.Model
{
    public class PlayerActionEventArgs : EventArgs
    {

        public Player player { get; private set; }
        public int ToX { get; private set; }
        public int ToY { get; private set; }
        public bool RightTurn { get; private set; }

        public PlayerActionEventArgs(Player player, int toX, int toY)
        {
            this.player = player;
            this.ToX = toX;
            this.ToY = toY;
        }
        public PlayerActionEventArgs(Player player)
        {
            this.player = player;
        }
        public PlayerActionEventArgs(Player player, bool rightTurn)
        {
            this.player= player;
            this.RightTurn = rightTurn;
        }
    }
}
