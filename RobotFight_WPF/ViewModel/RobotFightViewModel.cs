using RobotFight.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace RobotFight_WPF.ViewModel
{
    public class RobotFightViewModel : ViewModelBase
    {
        #region properties and fields

        private RobotFightModel _model;
        public ObservableCollection<GridImage> GridImages { get; set; }
        public ObservableCollection<String> HealthText { get; set; }
        public int BoardSize {
            get { return _model.BoardSize; }
        }

        public string RemainingActionsText {
            get
            {
                return $"Remaining actions for Player {_model.Turn+1} : {5 - _model.Players[_model.Turn].Actions.Count}";
            }
            private set { }
        }
        
        #endregion

        #region commands
        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand AddActionCommand {  get; private set; }
        public DelegateCommand ClearActionsCommand { get; private set; }
        public DelegateCommand LockInActionsCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }

        #endregion

        #region events
        public event EventHandler<EventArgs>? OnExitEvent;
        public event EventHandler<EventArgs>? OnSaveEvent;
        public event EventHandler<EventArgs>? OnLoadEvent;

        #endregion

        public RobotFightViewModel(RobotFightModel model)
        {
            _model = model;

            _model.Players[0].Moved += onPlayerMoved;
            _model.Players[1].Moved += onPlayerMoved;
            _model.TurnChanged += onTurnChanged;
            _model.Players[0].Hit += onPlayerHit;
            _model.Players[1].Hit += onPlayerHit;
            _model.PlayerTurned += onPlayerTurned;
            _model.GameOver += onGameEnded;
            _model.GameLoaded += onGameLoaded;
            _model.ActionAdded += (sender,args) => OnPropertyChanged(nameof(RemainingActionsText));

            NewGameCommand = new DelegateCommand(param => onNewGame(param));
            AddActionCommand = new DelegateCommand(param => onAddAction(param));
            ClearActionsCommand = new DelegateCommand(param => onClearActions());
            LockInActionsCommand = new DelegateCommand(param => onLockInActions());
            ExitCommand = new DelegateCommand(param => onExit());
            SaveGameCommand = new DelegateCommand(param => onSaveGame());
            LoadGameCommand = new DelegateCommand(param => onLoadGame());

            GridImages = new ObservableCollection<GridImage>();

            HealthText = new ObservableCollection<String>();
            HealthText.Add("Player 1 health : ♥ ♥ ♥");
            HealthText.Add("Player 2 health : ♥ ♥ ♥");
            
            initializeImages();
            addPlayers(false);
        }
        
        #region private methods
        private void initializeImages()
        {
            for (int i = 0; i < 64; i++)
            {
                GridImages.Add(new GridImage());
            }
        }
        private void addPlayers(bool fromLoad)
        {
            _model.ResetPlayers(fromLoad);
            int player1Position = _model.Players[0].Y * _model.BoardSize + _model.Players[0].X;
            int player2Position = _model.Players[1].Y * _model.BoardSize + _model.Players[1].X;
            GridImages[player1Position].Content = 1;
            GridImages[player2Position].Content = 2;
            switch (_model.Players[0].Orientation)
            {
                case RobotFight.Model.Orientation.Down: 
                    { 
                        break; 
                    }
                case RobotFight.Model.Orientation.Left:
                    {
                        GridImages[player1Position].Rotation = 90; break;
                    }
                case RobotFight.Model.Orientation.Up:
                    {
                        GridImages[player1Position].Rotation = 180; break;
                    }
                case RobotFight.Model.Orientation.Right:
                    {
                        GridImages[player1Position].Rotation = 270; break;
                    }
            }
            switch (_model.Players[1].Orientation)
            {
                case RobotFight.Model.Orientation.Down: { break; }
                case RobotFight.Model.Orientation.Left:
                    {
                        GridImages[player2Position].Rotation = 90; break;
                    }
                case RobotFight.Model.Orientation.Up:
                    {
                        GridImages[player2Position].Rotation = 180;
                        break;
                    }
                case RobotFight.Model.Orientation.Right:
                    {
                        GridImages[player2Position].Rotation = 270; break;
                    }
            }
            HealthText[0] = "Player 1 health : ♥ ♥ ♥";
            HealthText[1] = "Player 2 health : ♥ ♥ ♥";
        }
        private void movePlayer(Player player, int x, int y)
        {
            int positionFrom = player.Y * _model.BoardSize + player.X;
            int positionTo = y * _model.BoardSize + x;
            if (positionFrom != positionTo)
            {
                GridImages[positionTo].Content = GridImages[positionFrom].Content;
                GridImages[positionTo].Rotation = GridImages[positionFrom].Rotation;
                GridImages[positionFrom].Content = 0; GridImages[positionFrom].Rotation = 0;
            }
        }

        #endregion

        #region command handling
        private void onNewGame(object? param)
        {
            if(param is string sizeString)
            {
                _model.BoardSize = int.Parse(sizeString);
                OnPropertyChanged(nameof(BoardSize));
                for (int i = 0; i < BoardSize * BoardSize; i++)
                {
                    GridImages[i].Content = 0;
                    GridImages[i].Rotation = 0;
                }
                addPlayers(false);
            }
        }
        private void onAddAction(object? param)
        {
            if (param is string action)
            {
                Enum.TryParse(action, true, out GameAction gameAction);
                _model.AddAction(gameAction);
            }
        }
        private void onClearActions()
        {
            _model.ClearActionsCurrent();
            OnPropertyChanged(nameof(RemainingActionsText));
        }
        private void onLockInActions()
        {
            if (_model.Players[_model.Turn].Actions.Count != 5)
            {
                MessageBox.Show("Provide 5 inputs first", "Not enough inputs");
                return;
            }
            _model.TurnEnded();
            OnPropertyChanged(nameof(RemainingActionsText));
        }
        private void onExit()
        {
            OnExitEvent?.Invoke(this, new EventArgs());
        }
        private void onSaveGame()
        {
            OnSaveEvent?.Invoke(this, new EventArgs());
        }
        private void onLoadGame()
        {
            OnLoadEvent?.Invoke(this, new EventArgs());
        }

        #endregion

        #region event handling
        private void onPlayerMoved(object? sender, PlayerActionEventArgs args)
        {
            if (args.player == _model.Players[0])
            {
                movePlayer(_model.Players[0], args.ToX, args.ToY);
            }
            else if (args.player == _model.Players[1])
            {
                movePlayer(_model.Players[1], args.ToX, args.ToY);
            }
            else return;
        }
        private void onTurnChanged(object? sender, EventArgs args)
        {
            OnPropertyChanged(nameof(RemainingActionsText));
        }
        private void onPlayerHit(object? sender, EventArgs args)
        {
            if (sender == null) return;
            Player player = (Player)sender;
            int i = -1;
            if (player == _model.Players[0])
            {
                i = 0;
            }
            else if (player == _model.Players[1])
            {
                i = 1;
            }
            else return;
            string[] buffer = HealthText[i].Split(' ');
            HealthText[i] = "";

            for (int j = 0; j < buffer.Length - 1; j++)
            {
                HealthText[i] += buffer[j];
                if (j < buffer.Length - 2) { HealthText[i] += ' '; }
            }
        }
        private void onPlayerTurned(object? sender, PlayerActionEventArgs args)
        {
            if (args.player == null) return;
            int playerLocation = args.player.Y * _model.BoardSize + args.player.X;
            if (args.RightTurn)
            {
                GridImages[playerLocation].Rotation += 90;
            }
            else
            {
                GridImages[playerLocation].Rotation -= 90;
            }
        }
        private void onGameEnded(object? sender, GameEnd result)
        {
            MessageBox.Show(result.ToString() + '!', "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                GridImages[i].Content = 0;
                GridImages[i].Rotation = 0;
            }
            addPlayers(false);
        }
        private void onGameLoaded(object? sender, EventArgs args)
        {
            OnPropertyChanged(nameof(BoardSize));
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                GridImages[i].Content = 0;
                GridImages[i].Rotation = 0;
            }
            addPlayers(true);
        }

        #endregion
    }
}
