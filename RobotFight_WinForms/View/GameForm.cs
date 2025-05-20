using RobotFight.Model;
using RobotFight.Persistence;
using System.Numerics;
using System.Windows.Forms;
namespace RobotFight_WinForms
{
    public partial class GameForm : Form
    {
        #region fields

        private RobotFightModel model;

        private Button[]? mainMenuButtons;
        private MenuStrip topMenu = null!;

        private PictureBox[] board = null!;
        private TableLayoutPanel boardContainer = null!;
        private PictureBox[,] healthImages = null!;

        private readonly Image heartImage;
        private readonly Image[] robotImages;
        private readonly Font myFont;

        private Button[] actionButtons = null!;
        private Button lockInButton = null!;
        private Button clearButton = null!;

        private Label player1Label = null!;
        private Label player2Label = null!;
        private Label turnLabel = null!;
        private Label gameOverLabel = null!;

        #endregion
        public GameForm()
        {
            InitializeComponent();
            model = new RobotFightModel(new RobotFightFileAccess());
            myFont = new Font("Arial", 10, FontStyle.Bold);
            addMainMenu();
            setUpTopMenu();
            heartImage = Image.FromFile(Application.StartupPath + "\\Icons\\heart_small.jpg");
            setUpUI();
            robotImages =[Image.FromFile(Application.StartupPath + "\\Icons\\robot_1.png"), Image.FromFile(Application.StartupPath + "\\Icons\\robot_2.png")];

            model.Players[0].Moved += onPlayerMoved;
            model.Players[1].Moved += onPlayerMoved;
            model.TurnChanged += onTurnChanged;
            model.Players[0].Hit += onPlayerHit;
            model.Players[1].Hit += onPlayerHit;
            model.PlayerTurned += onPlayerTurned;
            model.GameOver += gameEnded;
            model.GameLoaded += loadGame;
            model.PlayTurnStarted += clearPlayerColor;
        }

        #region private methods

        private void addMainMenu()
        {
            Button new4x4Button = new Button() { Text = "New 4x4 game" };
            new4x4Button.Click += new4x4Game;
            Button new6x6Button = new Button() { Text = "New 6x6 game" };
            new6x6Button.Click += new6x6Game;
            Button new8x8Button = new Button() { Text = "New 8x8 game" };
            new8x8Button.Click += new8x8Game;
            Button loadGameButton = new Button() { Text = "Load game" };
            loadGameButton.Click += loadGameAsync;
            mainMenuButtons = new Button[]{new4x4Button, new6x6Button, new8x8Button, loadGameButton};
            int i = 0;
            foreach (Button button in mainMenuButtons) {
                button.Font = new Font("Impact", 20F);
                button.Location = new Point(0, i*70);
                button.Size = new Size(250, 70);
                button.TabIndex = i++;
            }
            Controls.AddRange(mainMenuButtons);
        }
        private void setUpTopMenu()
        {
            topMenu = new MenuStrip();

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            topMenu.Items.Add(fileMenu);
            ToolStripMenuItem newGameMenu = new ToolStripMenuItem("New Game");
            topMenu.Items.Add(newGameMenu);
            ToolStripMenuItem exitGame = new ToolStripMenuItem("Exit");
            topMenu.Items.Add(exitGame);
            exitGame.Click += closeGame;

            ToolStripMenuItem Save = new ToolStripMenuItem("Save game");
            fileMenu.DropDownItems.Add(Save);
            Save.Click += saveGameAsync;
            ToolStripMenuItem Load = new ToolStripMenuItem("Load game");
            fileMenu.DropDownItems.Add(Load);
            Load.Click += loadGameAsync;

            ToolStripMenuItem new4x4GameMenuItem = new ToolStripMenuItem("New 4x4 game");
            newGameMenu.DropDownItems.Add(new4x4GameMenuItem);
            new4x4GameMenuItem.Click += new4x4Game;
            ToolStripMenuItem new6x6GameMenuItem = new ToolStripMenuItem("New 6x6 game");
            newGameMenu.DropDownItems.Add(new6x6GameMenuItem);
            new6x6GameMenuItem.Click += new6x6Game;
            ToolStripMenuItem new8x8GameMenuItem = new ToolStripMenuItem("New 8x8 game");
            newGameMenu.DropDownItems.Add(new8x8GameMenuItem);
            new8x8GameMenuItem.Click += new8x8Game;
        }
        private void removeMainMenu()
        {
            if (mainMenuButtons != null)
            {
                foreach (Button button in mainMenuButtons)
                {
                    Controls.Remove(button);
                }
                mainMenuButtons = null;
            }
        }
        private void addBoard()
        {
            switch (model.BoardSize)
            {
                case 4:
                    {
                        this.Size = new Size(800, 550); break;
                    }
                case 6:
                    {
                        this.Size = new Size(900, 700); break;
                    }
                case 8:
                    {
                        this.Size = new Size(1000, 850); break;
                    }
                default :
                    this.Size = new Size(500, 500); break;
            }
            board = new PictureBox[model.BoardSize*model.BoardSize];
            CenterToScreen();

            if(boardContainer!=null && Controls.Contains(boardContainer)) { Controls.Remove(boardContainer); }
            boardContainer = new TableLayoutPanel();
            boardContainer.BackColor = Color.Lavender;
            boardContainer.RowCount = model.BoardSize;
            boardContainer.ColumnCount = boardContainer.RowCount;

            for (int i = 0; i < board.Length; i++) {
                board[i] = new PictureBox();
                board[i].Margin = new Padding(3);
                board[i].BorderStyle = BorderStyle.FixedSingle;
                board[i].Size = new Size(robotImages[0].Width + 2 * (int)BorderStyle.FixedSingle, robotImages[0].Height+2*(int)BorderStyle.FixedSingle);
            }
            boardContainer.Size = new Size(boardContainer.ColumnCount * (board[0].Width + 2 * board[0].Margin.All), boardContainer.RowCount * (board[0].Height + 2 * board[0].Margin.All));
            boardContainer.Location = new Point((ClientSize.Width - boardContainer.Width) / 2, 30);

            boardContainer.Controls.AddRange(board);
            Controls.Add(boardContainer);
        }
        private void addPlayers()
        {
            int player1Location = model.Players[0].Y * model.BoardSize + model.Players[0].X;
            int player2Location = model.Players[1].Y * model.BoardSize + model.Players[1].X;
            board[player1Location].Image = robotImages[0];
            board[player2Location].Image = robotImages[1];
            switch (model.Players[0].Orientation) 
            {
                case RobotFight.Model.Orientation.Down: { break; }
                case RobotFight.Model.Orientation.Left:
                    {
                        board[player1Location].Image = rotateImage(board[player1Location].Image,RotateFlipType.Rotate90FlipNone); break;
                    }
                case RobotFight.Model.Orientation.Up:
                    {
                        board[player1Location].Image = rotateImage(board[player1Location].Image, RotateFlipType.Rotate180FlipNone); break;
                    }
                case RobotFight.Model.Orientation.Right:
                    {
                        board[player1Location].Image = rotateImage(board[player1Location].Image, RotateFlipType.Rotate270FlipNone); break;
                    }
            }
            switch (model.Players[1].Orientation)
            {
                case RobotFight.Model.Orientation.Down: { break; }
                case RobotFight.Model.Orientation.Left:
                    {
                        board[player2Location].Image = rotateImage(board[player2Location].Image, RotateFlipType.Rotate90FlipNone); break;
                    }
                case RobotFight.Model.Orientation.Up:
                    {
                        board[player2Location].Image = rotateImage(board[player2Location].Image, RotateFlipType.Rotate180FlipNone); break;
                    }
                case RobotFight.Model.Orientation.Right:
                    {
                        board[player2Location].Image = rotateImage(board[player2Location].Image, RotateFlipType.Rotate270FlipNone); break;
                    }
            }

        }
        private Image rotateImage(Image image, RotateFlipType rotateFlip)
        {
            Bitmap bmp = new Bitmap(image);
            bmp.RotateFlip(rotateFlip);
            return bmp;
        }
        private void setUpUI()
        {
            healthImages = new PictureBox[2, model.Players[0].BaseHealth];
            for (int i = 0; i < healthImages.GetLength(1); i++) { 
                healthImages[0,i] = new PictureBox();
                healthImages[0,i].Size = new Size(50, 50);
                healthImages[0,i].Image = heartImage;
                

                healthImages[1,i] = new PictureBox();
                healthImages[1,i].Image = heartImage;
                healthImages[1,i].Size = new Size(50, 50);
            }
            player1Label = new Label();
            player1Label.Font = myFont;
            player1Label.Text = "Player 1 - orange";
            player2Label = new Label();
            player2Label.Font = myFont;
            player2Label.Text = "Player 2 - red";
            Button moveFowardButton = new Button() { Text = "Move Forward" };
            Button moveRightButton = new Button() { Text = "Move Right" };
            Button moveBackButton = new Button() { Text = "Move Back" };
            Button moveLeftButton = new Button() { Text = "Move Left"};
            Button turnRightButton = new Button() { Text = "Turn Right" };
            Button turnLeftButton = new Button() { Text = "Turn Left" };
            Button laserButton = new Button() { Text = "Use Laser" };
            Button meleeButton = new Button() { Text = "Melee Attack" };
            actionButtons = new Button[] { moveFowardButton,moveRightButton,moveBackButton,moveLeftButton,turnRightButton,turnLeftButton,laserButton,meleeButton };
            foreach(Button button in actionButtons)
            {
                button.Click += actionButtonPressed;
                button.Font = myFont;
                button.Size = new Size(150, 50);
            }
            clearButton = new Button() { Text = "Clear actions", Font = myFont, Size = new Size(200, 50) };
            clearButton.Click += clearActions;
            lockInButton = new Button() { Text = "Lock in actions" , Font = myFont, Size = new Size(200,50)};
            lockInButton.Click += lockInActions;
            turnLabel = new Label() { Text = "Player 1's turn" , Font = myFont};
            gameOverLabel = new Label() { Font = new Font("Impact", 20), TextAlign = ContentAlignment.MiddleCenter};
        }
        private void addUI()
        {
            for (int i = 0; i < healthImages.GetLength(1); i++)
            {
                healthImages[0, i].Location = new Point(i * 50, ClientSize.Height - 120);
                Controls.Add(healthImages[0, i]);
                healthImages[1, i].Location = new Point(ClientSize.Width - (i + 1) * healthImages[1, i].Width, ClientSize.Height - 120);
                Controls.Add(healthImages[1, i]);
            }

            player1Label.Location = new Point(0, healthImages[0, 0].Location.Y + 70);
            player1Label.Size = new Size(healthImages[0, 0].Width * 3, 50);
            Controls.Add(player1Label);

            player2Label.Location = new Point(ClientSize.Width - 3 * healthImages[0, 0].Width, healthImages[0, 0].Location.Y + 70);
            player2Label.Size = new Size(healthImages[0, 0].Width * 3, 50);
            Controls.Add(player2Label);

            for (int i = 0; i < 4; i++)
            {
                actionButtons[i].Location = new Point((boardContainer.Location.X - actionButtons[i].Width) / 2, 50 + i * (ClientSize.Height / 7));
            }
            for (int i = 0; i < 4; i++)
            {
                actionButtons[i + 4].Location = new Point(ClientSize.Width - (boardContainer.Location.X - actionButtons[i + 4].Width) / 2 - actionButtons[i + 4].Width, 50 + i * (ClientSize.Height / 7));
            }
            Controls.AddRange(actionButtons);
            clearButton.Location = new Point((ClientSize.Width - clearButton.Width) / 2, ClientSize.Height-120);
            Controls.Add(clearButton);
            lockInButton.Location = new Point((ClientSize.Width - lockInButton.Width) / 2, ClientSize.Height - 60);
            Controls.Add(lockInButton);
            turnLabel.Size = new Size(lockInButton.Width, 30);
            turnLabel.Location = new Point((ClientSize.Width - turnLabel.Width) / 2, ClientSize.Height - 170);
            turnLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(turnLabel);
        }
        private void enterGameScene()
        {
            if (mainMenuButtons!=null)
            {
                removeMainMenu();
                Controls.Add(topMenu);
            }
            if (Controls.Contains(gameOverLabel)) { Controls.Remove(gameOverLabel); }
            hideButtons();
            addBoard();
            model.ResetPlayers(false);
            addPlayers();
            clearActionButtons();
            addUI();
            showButtons();
        }

        private void movePlayer(Player player, int toX, int toY) 
        {
            if (player.Y >= model.BoardSize || player.X >= model.BoardSize
                || (player.Y == toY && player.X == toX) )
            {
                return;
            }
            else
            {
                board[toY * model.BoardSize + toX].Image = board[player.Y * model.BoardSize + player.X].Image;
                board[player.Y * model.BoardSize + player.X].Image = null;
                board[toY * model.BoardSize + toX].Refresh();
                board[player.Y * model.BoardSize + player.X].Refresh();
            }
        }

        private void clearActionButtons()
        {
            foreach (Button button in actionButtons)
            {
                string[] buff = button.Text.Split();
                button.Text = buff[0] + ' ' + buff[1];
            }
        }
        private void hideButtons()
        {
            foreach (Control control in Controls)
            {
                if (control is Button)
                {
                    control.Hide();
                }
            }
        }
        private void showButtons()
        {
            foreach (Control control in Controls)
            {
                if (control is Button)
                {
                    control.Show();
                }
            }
        }

        #endregion

        #region eventhandlers
        private void onPlayerHit(object? e, EventArgs args) 
        {
            if(e == null ) return;
            Player player = (Player)e;
            int i;
            if (player == model.Players[0])
            {
                i = 0;
            }
            else if (player == model.Players[1])
            {
                i = 1;
            }
            else return;
            /*board[player.Y * model.BoardSize + player.X].BackColor = Color.Red;
            board[player.Y * model.BoardSize + player.X].Refresh();*/
            int j = model.Players[i].BaseHealth;
            while (j > model.Players[i].Health)
            {
                Controls.Remove(healthImages[i, j-1]);
                j--;
            }
        }
        private void onPlayerMoved(object? e, PlayerActionEventArgs args)
        {
            if (args.player == model.Players[0])
            {
                movePlayer(model.Players[0],args.ToX,args.ToY);
            }
            else if (args.player == model.Players[1])
            {
                movePlayer(model.Players[1], args.ToX, args.ToY);
            }
            else return;
        }
        private void onTurnChanged(object? e, EventArgs args) 
        { 
            if(model.Turn == 1) { turnLabel.Text = "Player 2's turn"; }
            else if (model.Turn == 0) { turnLabel.Text = "Player 1's turn"; }
        }
        private void clearPlayerColor(object? e, EventArgs args)
        {
            board[model.Players[0].Y * model.BoardSize + model.Players[0].X].BackColor = default;
            board[model.Players[1].Y * model.BoardSize + model.Players[1].X].BackColor = default;
        }
        private void onPlayerTurned(object? e, PlayerActionEventArgs args)
        {
            if (args.player == null) return;
            int playerLocation = args.player.Y * model.BoardSize + args.player.X;
            if (args.RightTurn)
            {
                board[playerLocation].Image = rotateImage(board[playerLocation].Image, RotateFlipType.Rotate90FlipNone);
            }
            else
            {
                board[playerLocation].Image = rotateImage(board[playerLocation].Image, RotateFlipType.Rotate270FlipNone);
            }
            board[playerLocation].Refresh();
        }
        private void gameEnded(object? sender, GameEnd winner)
        {
            hideButtons();
            gameOverLabel.Size = new Size(ClientSize.Width / 2, 100);
            gameOverLabel.Location = new Point(ClientSize.Width / 4, ClientSize.Height - gameOverLabel.Height);
            switch (winner)
            {
                case GameEnd.Tie:
                    {
                        gameOverLabel.Text = "It's a tie!";
                        break;
                    }
                case GameEnd.Player1Won:
                    {
                        gameOverLabel.Text = "Player 1 Won!";
                        break;
                    }
                case GameEnd.Player2Won:
                    {
                        gameOverLabel.Text = "Player 2 Won!";
                        break;
                    }
            }
            Controls.Add(gameOverLabel);
        }

        #endregion

        #region local eventhandlers
        private void new4x4Game(object? sender, EventArgs e)
        {
            model.BoardSize = 4;
            enterGameScene();
        }
        private void new6x6Game(object? sender, EventArgs e)
        {
            model.BoardSize = 6;
            enterGameScene();
        }
        private void new8x8Game(object? sender, EventArgs e)
        {
            model.BoardSize = 8;
            enterGameScene();
        }
        private void closeGame(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Click \"OK\" to exit the game, cancel to continue playing", "Exit game", MessageBoxButtons.OKCancel) == DialogResult.OK) { 
                    this.Close(); 
            }
        }
        private void actionButtonPressed(object? sender, EventArgs e)
        {
            if(sender != null)
            {
                Button button = (Button)sender;
                GameAction action = GameAction.MoveForward;
                if(button == actionButtons[0]) { action = GameAction.MoveForward; }
                else if(button == actionButtons[1]) { action = GameAction.MoveRight; }
                else if (button == actionButtons[2]) { action = GameAction.MoveBack; }
                else if (button == actionButtons[3]) { action = GameAction.MoveLeft; }
                else if (button == actionButtons[4]) { action = GameAction.TurnRight; }
                else if (button == actionButtons[5]) { action = GameAction.TurnLeft; }
                else if (button == actionButtons[6]) { action = GameAction.UseLaser; }
                else if (button == actionButtons[7]) { action = GameAction.Melee; }
                if (model.AddAction(action))
                {
                    button.Text += $" ({model.Players[model.Turn].Actions.Count})";
                }
            }
        }
        
        private void lockInActions(object? sender, EventArgs e)
        {
            if (model.Players[model.Turn].Actions.Count != 5)
            {
                MessageBox.Show("Provide 5 inputs first","Not enough inputs");
                return;
            }
            clearActionButtons();
            if(model.Turn == 1)
            {
                
                topMenu.Hide();
                hideButtons();
            }
            model.TurnEnded();
            if (!model.IsGameOver)
            {
                showButtons();
            }
            topMenu.Show();
        }
        private void clearActions(object? sender, EventArgs e)
        {
            clearActionButtons();
            model.ClearActionsCurrent();
        }
        
        private async void saveGameAsync(object? sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await model.SaveGameAsync(saveFileDialog.FileName);
                }
                catch (Exception) { MessageBox.Show("Saving game failed", "Save failed", MessageBoxButtons.OK); }
            }
        }
        private async void loadGameAsync(object? sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await model.LoadGameAsync(openFileDialog.FileName);
                }
                catch (Exception) {
                    MessageBox.Show("Loading game failed", "Load failed", MessageBoxButtons.OK);
                    model.BoardSize = 4;
                    enterGameScene();
                }
            }
        }
        private void loadGame(object? sender, EventArgs e)
        {
            if (mainMenuButtons != null)
            {
                removeMainMenu();
                Controls.Add(topMenu);
            }
            if (Controls.Contains(gameOverLabel)) { Controls.Remove(gameOverLabel); }
            addBoard();
            addUI();
            clearActionButtons();
            model.ResetPlayers(true);
            addPlayers();
            showButtons();
        }
        #endregion
    }
}

