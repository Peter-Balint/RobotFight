using RobotFight.Model;
using System.Collections.ObjectModel;
using System;

namespace RobotFight_Avalonia.ViewModels
{
    public partial class RobotFightViewModel : ViewModelBase
    {
        #region properties and fields

        private RobotFightModel _model;
        public ObservableCollection<GridImage> GridImages { get; set; }
        public ObservableCollection<String> HealthText { get; set; }
        public int BoardSize
        {
            get { return _model.BoardSize; }
        }

        public string CurrentActionsText
        {
            get
            {
                string list = $"Player {_model.Turn + 1} current inputs: ";
                foreach(GameAction action in _model.Players[_model.Turn].Actions)
                {
                    list += action.ToString() + " | ";
                }
                return list;
            }
        }

        #endregion

        #region commands
        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand AddActionCommand { get; private set; }
        public DelegateCommand ClearActionsCommand { get; private set; }
        public DelegateCommand LockInActionsCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }

        #endregion

        #region events
        public event EventHandler<EventArgs>? OnSaveEvent;
        public event EventHandler<EventArgs>? OnLoadEvent;

        public event EventHandler<EventArgs>? OnInsufficientActionsEvent;

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
            _model.ActionAdded += (sender, args) => OnPropertyChanged(nameof(CurrentActionsText));

            NewGameCommand = new DelegateCommand(param => onNewGame(param));
            AddActionCommand = new DelegateCommand(param => onAddAction(param));
            ClearActionsCommand = new DelegateCommand(param => onClearActions());
            LockInActionsCommand = new DelegateCommand(param => onLockInActions());
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
            GridImages[player1Position].IsRobot1 = true;
            GridImages[player2Position].IsRobot2 = true;
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
                GridImages[positionTo].IsRobot1 = GridImages[positionFrom].IsRobot1;
                GridImages[positionTo].IsRobot2 = GridImages[positionFrom].IsRobot2;
                GridImages[positionTo].Rotation = GridImages[positionFrom].Rotation;
                GridImages[positionFrom].IsRobot1 = GridImages[positionFrom].IsRobot2 = false;
                GridImages[positionFrom].Rotation = 0;
            }
        }

        #endregion

        #region command handling
        private void onNewGame(object? param)
        {
            if (param is string sizeString)
            {
                _model.BoardSize = int.Parse(sizeString);
                OnPropertyChanged(nameof(BoardSize));
                for (int i = 0; i < BoardSize * BoardSize; i++)
                {
                    GridImages[i].IsRobot1 = GridImages[i].IsRobot2 = false;
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
            OnPropertyChanged(nameof(CurrentActionsText));
        }
        private void onLockInActions()
        {
            if (_model.Players[_model.Turn].Actions.Count != 5)
            {
                OnInsufficientActionsEvent?.Invoke(this,EventArgs.Empty);
                return;
            }
            _model.TurnEnded();
            OnPropertyChanged(nameof(CurrentActionsText));
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
            OnPropertyChanged(nameof(CurrentActionsText));
        }
        private void onPlayerHit(object? sender, EventArgs args)
        {
            if (sender == null) return;
            Player player = (Player)sender;
            int i;
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
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                GridImages[i].IsRobot1 = GridImages[i].IsRobot2 = false;
                GridImages[i].Rotation = 0;
            }
            addPlayers(false);
        }
        private void onGameLoaded(object? sender, EventArgs args)
        {
            OnPropertyChanged(nameof(BoardSize));
            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                GridImages[i].IsRobot1 = GridImages[i].IsRobot2 = false;
                GridImages[i].Rotation = 0;
            }
            addPlayers(true);
        }

        #endregion
    }
}
