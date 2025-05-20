using RobotFight.Model;
using RobotFight.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
//using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RobotFight.Model
{
    
    public enum GameEnd { Player1Won, Player2Won, Tie}
    public class RobotFightModel
    {
        #region fields

        private int boardSize;
        private int turn;
        private bool isGameOver;
        private Player[] players;
        private IRobotFightDataAccess dataAccess;

        #endregion

        #region properties

        public int BoardSize{
            get { return boardSize; }
            set
            {
                if (value == 4 || value == 6 || value == 8)
                    boardSize = value;
                else throw new ArgumentException();
            }
        }
        public int Turn {
            get { return turn; }
            set
            {
                if (value == 0 || value == 1) { 
                    turn = value; 
                    TurnChanged?.Invoke(this, EventArgs.Empty);
                }
                else throw new ArgumentException();
            }
        }
        public bool IsGameOver { get { return isGameOver; } private set { isGameOver = value; } }
        public Player[] Players { get { return players; } private set { players = value; } }

        #endregion

        #region events

        public event EventHandler<EventArgs>? TurnChanged;
        public event EventHandler<PlayerActionEventArgs>? PlayerTurned;
        public event EventHandler<GameEnd>? GameOver; 
        public event EventHandler<EventArgs>? GameLoaded;
        public event EventHandler<EventArgs>? PlayTurnStarted;
        public event EventHandler<EventArgs>? ActionAdded;
        #endregion

        public RobotFightModel(IRobotFightDataAccess robotFightDataAccess)
        {
            dataAccess = robotFightDataAccess;
            boardSize = 4;
            IsGameOver = false;
            Players = new Player[2];
            Players[0] = new Player();
            Players[1] = new Player();
            players = Players;
        }

        #region public methods
        public void ResetPlayers(bool fromLoad)
        {
            if (!fromLoad)
            {
                Players[0].BoardSize = BoardSize;
                Players[0].Orientation = Orientation.Down;
                Players[0].Health = Players[0].BaseHealth;
                Players[1].BoardSize = BoardSize;
                Players[1].Orientation = Orientation.Up;
                Players[1].Health = Players[1].BaseHealth;
            }
            Players[0].SetPosition(BoardSize / 2 - 1, 0);
            Players[1].SetPosition(BoardSize / 2, BoardSize - 1);

            clearActionsAll();
            Turn = 0;
        }
        public bool AddAction(GameAction action)
        {
            if (Players[Turn].Actions.Count == 5) { return false; }
            Players[Turn].Actions.Add(action);
            ActionAdded?.Invoke(this, EventArgs.Empty);
            return true;
        } 
        public void TurnEnded()
        {
            if (Turn == 0) { Turn = 1; }
            else
            {
                playGame();
            }
        }
        public void ClearActionsCurrent()
        {
            Players[Turn].Actions.Clear();
        }

        public async Task SaveGameAsync(string path)
        {
            await SaveGameAsync(File.OpenWrite(path));
        }
        public async Task SaveGameAsync(Stream stream)
        {
            if (dataAccess == null) { throw new Exception(); }
            PlayerData pd1 = new PlayerData(Players[0].X, Players[0].Y, Players[0].Health, BoardSize, Players[0].Orientation);
            PlayerData pd2 = new PlayerData(Players[1].X, Players[1].Y, Players[1].Health, BoardSize, Players[1].Orientation);
            await dataAccess.SaveAsync(stream, [pd1, pd2]);
        }

        public async Task LoadGameAsync(String path)
        {
            await LoadGameAsync(File.OpenRead(path));
        }
        public async Task LoadGameAsync(Stream stream)
        {
            if (dataAccess == null) { throw new Exception(); }

            players[0].Health = players[0].BaseHealth;
            players[1].Health = players[1].BaseHealth;

            PlayerData[] pd = await dataAccess.LoadAsync(stream);

            boardSize = pd[0].BoardSize;
            Players[0].BoardSize = pd[0].BoardSize;
            Players[1].BoardSize = pd[1].BoardSize;
            Players[0].Orientation = pd[0].Orientation;
            Players[1].Orientation = pd[1].Orientation;

            GameLoaded?.Invoke(this, new EventArgs());

            Players[0].SetPosition(pd[0].X, pd[0].Y);
            Players[1].SetPosition(pd[1].X, pd[1].Y);
            while (Players[0].Health > pd[0].Health) { players[0].Health--; }
            while (Players[1].Health > pd[1].Health) { players[1].Health--; }


            PlayTurnStarted?.Invoke(this, new EventArgs());

            clearActionsAll();
            Turn = 0;
        }

        #endregion

        #region private methods
        private void clearActionsAll()
        {
            Players[0].Actions.Clear();
            Players[1].Actions.Clear();
        }
        private void playGame()
        {
            int i = 0;
            IsGameOver = false;
            while (i < Math.Min(Players[0].Actions.Count, Players[1].Actions.Count) && !isGameOver) {
                if (isMovement(Players[0].Actions[i]) && isMovement(Players[1].Actions[i]))
                {
                    doubleMovement(i);
                }
                else if (isMovement(Players[0].Actions[i]))
                {
                    (int x, int y) = movePlayer(Players[0], Players[0].Actions[i]);
                    if (!(x == Players[1].X && y == Players[1].Y))
                    {
                        Players[0].SetPosition(x, y);
                    }
                    nonMovementAction(Players[1], Players[1].Actions[i], Players[0]);
                }
                else if (isMovement(Players[1].Actions[i]))
                {
                    (int x, int y) = movePlayer(Players[1], Players[1].Actions[i]);
                    if (!(x == Players[0].X && y == Players[0].Y))
                    {
                        Players[1].SetPosition(x, y);
                    }
                    nonMovementAction(Players[0], Players[0].Actions[i], Players[1]);
                }
                else
                {
                    nonMovementAction(Players[0], Players[0].Actions[i], Players[1]);
                    nonMovementAction(Players[1], Players[1].Actions[i], Players[0]);
                }

                if (Players[0].Health == 0 && Players[1].Health == 0) 
                {
                    GameOver?.Invoke(this, GameEnd.Tie);
                    IsGameOver = true;
                }
                else if (Players[0].Health == 0)
                {
                    GameOver?.Invoke(this, GameEnd.Player2Won);
                    IsGameOver = true;
                }
                else if (Players[1].Health == 0)
                {
                    GameOver?.Invoke(this, GameEnd.Player1Won);
                    IsGameOver = true;
                }
                i++;
            }
            clearActionsAll();
            Turn = 0;
        }
        private void nonMovementAction(Player player, GameAction action, Player otherPlayer)
        {
            switch (action) 
            {
                case GameAction.TurnRight:
                    {
                        turnPlayerRight(player);
                        break;
                    }
                case GameAction.TurnLeft:
                    {
                        turnPlayerLeft(player);
                        break;
                    }
                case GameAction.UseLaser:
                    {
                        useLaser(player, otherPlayer);
                        break;
                    }
                case GameAction.Melee:
                    {
                        useMelee(player, otherPlayer);
                        break;
                    }
            }
        }
        private void turnPlayerRight(Player player)
        {
            player.Orientation = (Orientation)(((int)player.Orientation + 1) % 4);
            PlayerTurned?.Invoke(this, new PlayerActionEventArgs(player, true));
        }
        private void turnPlayerLeft(Player player)
        {
            player.Orientation = (Orientation)(((int)player.Orientation + 3) % 4);
            PlayerTurned?.Invoke(this, new PlayerActionEventArgs(player, false));
        }
        private (int,int) movePlayer(Player player, GameAction move)
        {
            if (    (player.Orientation == Orientation.Up && move == GameAction.MoveForward) ||
                    (player.Orientation == Orientation.Down && move == GameAction.MoveBack) ||
                    (player.Orientation == Orientation.Left && move == GameAction.MoveRight) ||
                    (player.Orientation == Orientation.Right && move == GameAction.MoveLeft)
                ){ 
                return(player.X, player.Y-1);
            }
            else if ((player.Orientation == Orientation.Up && move == GameAction.MoveBack) ||
                    (player.Orientation == Orientation.Down && move == GameAction.MoveForward) ||
                    (player.Orientation == Orientation.Left && move == GameAction.MoveLeft) ||
                    (player.Orientation == Orientation.Right && move == GameAction.MoveRight)
                ){
                return (player.X, player.Y + 1);
            }
            else if ((player.Orientation == Orientation.Up && move == GameAction.MoveLeft) ||
                    (player.Orientation == Orientation.Down && move == GameAction.MoveRight) ||
                    (player.Orientation == Orientation.Left && move == GameAction.MoveForward) ||
                    (player.Orientation == Orientation.Right && move == GameAction.MoveBack)
                ){
                return (player.X - 1, player.Y);
            }
            else if ((player.Orientation == Orientation.Up && move == GameAction.MoveRight) ||
                    (player.Orientation == Orientation.Down && move == GameAction.MoveLeft) ||
                    (player.Orientation == Orientation.Left && move == GameAction.MoveBack) ||
                    (player.Orientation == Orientation.Right && move == GameAction.MoveForward)
                ){
                return (player.X + 1, player.Y);
            }
            else return (0,0);
        } 
        private bool isMovement(GameAction action)
        {
            if (action == GameAction.MoveRight || action == GameAction.MoveLeft || action == GameAction.MoveForward || action == GameAction.MoveBack)
            {
                return true;
            }
            else return false;
        }
        private void useMelee(Player user, Player otherPlayer)
        {
            if (otherPlayer.X >= user.X - 1 && otherPlayer.X <= user.X + 1 && otherPlayer.Y >= user.Y - 1 && otherPlayer.Y <= user.Y + 1) 
            {
                otherPlayer.Health--;
            }
        }
        private void useLaser(Player user, Player otherPlayer)
        {
            switch (user.Orientation) 
            {
                case Orientation.Left:
                    {
                        if (otherPlayer.Y == user.Y && otherPlayer.X < user.X)
                        {
                            otherPlayer.Health--;
                        }
                        break;
                    }
                case Orientation.Right:
                    {
                        if(otherPlayer.Y == user.Y && otherPlayer.X > user.X)
                        {
                            otherPlayer.Health--;
                        }
                        break;
                    }
                case Orientation.Up:
                    {
                        if (otherPlayer.X == user.X && otherPlayer.Y < user.Y)
                        {
                            otherPlayer.Health--;
                        }
                        break;
                    }
                case Orientation.Down:
                    {
                        if (otherPlayer.X == user.X && otherPlayer.Y > user.Y)
                        {
                            otherPlayer.Health--;
                        }
                        break;
                    }
            }
        }
        private void doubleMovement(int actionNum)
        {
            int player1MoveX; int player1MoveY;
            int player2MoveX; int player2MoveY;
            bool player1MoveWrong = false;
            bool player2MoveWrong = false;
            (player1MoveX, player1MoveY) = movePlayer(Players[0], Players[0].Actions[actionNum]);
            (player2MoveX, player2MoveY) = movePlayer(Players[1], Players[1].Actions[actionNum]);
            if (player1MoveX == player2MoveX && player1MoveY == player2MoveY) 
            { 
                player1MoveWrong = true;
                player2MoveWrong = true;
            }
            if (player1MoveX == Players[1].X && player1MoveY == Players[1].Y) 
            {
                player1MoveWrong = true;
            }
            if (player2MoveX == Players[0].X && player2MoveY == Players[0].Y) 
            {
                player2MoveWrong = true;
            }
            if (!player1MoveWrong)
            {
                Players[0].SetPosition(player1MoveX, player1MoveY);
            }
            if (!player2MoveWrong)
            {
                Players[1].SetPosition(player2MoveX, player2MoveY);
            }
        }

        #endregion

    }
}
