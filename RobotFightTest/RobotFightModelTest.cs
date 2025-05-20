using Moq;
using RobotFight.Model;
using RobotFight.Persistence;
using System.Reflection;

namespace RobotFightTest
{
    [TestClass]
    public class RobotFightModelTest
    {
        private RobotFightModel _model = null!;
        private Mock<IRobotFightDataAccess> _mock = null!;
        private PlayerData _player1 = null!;
        private PlayerData _player2 = null!;

        [TestInitialize]
        public void Initialize()
        {
            _player1 = new PlayerData(1, 1, 3, 4, Orientation.Right);
            _player2 = new PlayerData(2, 1, 3, 4, Orientation.Left);

            _mock = new Mock<IRobotFightDataAccess>();
            _mock.Setup(m => m.LoadAsync(It.IsAny<Stream>())).Returns(() => Task.FromResult<PlayerData[]>([_player1, _player2]));

            _model = new RobotFightModel(_mock.Object);
        }

        [TestMethod]
        public async Task TestLoadGameAsync()
        {
            bool gameLoaded = false;
            _model.GameLoaded += (sender, args) => gameLoaded = true;

            var stream = new MemoryStream();
            await _model.LoadGameAsync(stream);

            Assert.AreEqual(_player1.Health, _model.Players[0].Health);
            Assert.AreEqual(_player1.BoardSize, _model.Players[0].BoardSize);
            Assert.AreEqual(_player1.X, _model.Players[0].X);
            Assert.AreEqual(_player1.Y, _model.Players[0].Y);
            Assert.AreEqual(_player1.Orientation, _model.Players[0].Orientation);
            Assert.AreEqual(0, _model.Players[0].Actions.Count);

            Assert.AreEqual(_player2.Health, _model.Players[1].Health);
            Assert.AreEqual(_player2.BoardSize, _model.Players[1].BoardSize);
            Assert.AreEqual(_player2.X, _model.Players[1].X);
            Assert.AreEqual(_player2.Y, _model.Players[1].Y);
            Assert.AreEqual(_player2.Orientation, _model.Players[1].Orientation);
            Assert.AreEqual(0, _model.Players[1].Actions.Count);

            Assert.IsTrue(gameLoaded);
        }
        [TestMethod]
        public void TestResetPlayers()
        {
            _model.ResetPlayers(false);
            Assert.AreEqual(_model.Players[0].BaseHealth, _model.Players[0].Health);
            Assert.AreEqual(_model.BoardSize, _model.Players[0].BoardSize);
            Assert.AreEqual(_model.BoardSize / 2 - 1, _model.Players[0].X);
            Assert.AreEqual(0, _model.Players[0].Y);
            Assert.AreEqual(Orientation.Down, _model.Players[0].Orientation);
            Assert.AreEqual(0, _model.Players[0].Actions.Count);

            Assert.AreEqual(_model.Players[1].BaseHealth, _model.Players[1].Health);
            Assert.AreEqual(_model.BoardSize, _model.Players[1].BoardSize);
            Assert.AreEqual(_model.BoardSize / 2, _model.Players[1].X);
            Assert.AreEqual(3, _model.Players[1].Y);
            Assert.AreEqual(Orientation.Up, _model.Players[1].Orientation);
            Assert.AreEqual(0, _model.Players[1].Actions.Count);
        }
        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void TestTurnChange()
        {
            _model.Turn = 0;
            bool turnChanged = false;
            _model.TurnChanged += (sender, args) => turnChanged = true;

            _model.Turn = 10;
            Assert.IsFalse(turnChanged);

            _model.TurnEnded();
            Assert.IsTrue(turnChanged);
        }
        [TestMethod]
        public async Task TestSetPosition()
        {
            var stream = new MemoryStream();
            await _model.LoadGameAsync(stream);

            bool player1MoveEvent = false;
            bool player2MoveEvent = false;
            _model.Players[0].Moved += (sender, args) => player1MoveEvent = true;
            _model.Players[1].Moved += (sender, args) => player2MoveEvent = true;

            _model.Players[0].SetPosition(0, 0);
            _model.Players[1].SetPosition(_model.BoardSize, 1);

            Assert.AreEqual(0, _model.Players[0].X);
            Assert.AreEqual(0, _model.Players[0].Y);

            Assert.AreEqual(_player2.X, _model.Players[1].X);
            Assert.AreEqual(_player2.Y, _model.Players[1].Y);

            Assert.IsTrue(player1MoveEvent);
            Assert.IsFalse(player2MoveEvent);
        }
        [TestMethod]
        public void TestAddAction()
        {
            bool actionAdded;
            _model.ActionAdded += (sender, args) => actionAdded = true;
            _model.Players[0].Actions.AddRange([GameAction.UseLaser, GameAction.UseLaser, GameAction.UseLaser, GameAction.UseLaser, GameAction.UseLaser]);
            actionAdded = false;
            _model.Turn = 0;
            _model.AddAction(GameAction.UseLaser);
            Assert.IsFalse(actionAdded);
            _model.Turn = 1;
            _model.AddAction(GameAction.UseLaser);
            Assert.IsTrue(actionAdded);
        }
        
        [TestMethod]
        public async Task TestMoveToEachOther()
        {
            var stream = new MemoryStream();
            await _model.LoadGameAsync(stream);
            _model.Turn = 1;

            _model.Players[0].Actions.Add(GameAction.MoveForward);
            _model.Players[1].Actions.Add(GameAction.MoveForward);

            _model.TurnEnded();

            Assert.AreEqual(_player1.X, _model.Players[0].X);
            Assert.AreEqual(_player1.Y, _model.Players[0].Y);
            Assert.AreEqual(_player2.X, _model.Players[1].X);
            Assert.AreEqual(_player2.Y, _model.Players[1].Y);
        }
        [TestMethod]
        public async Task TestTurnPlayers()
        {
            var stream = new MemoryStream();
            await _model.LoadGameAsync(stream);

            bool player1Turned = false;
            bool player2Turned = false;
            _model.PlayerTurned += (sender, args) =>
            {
                if (args.player == _model.Players[0]) { player1Turned = true; }
                else if(args.player == _model.Players[1]) {  player2Turned = true; }
            };

            _model.Players[0].Actions.Add(GameAction.TurnLeft);
            _model.Players[1].Actions.Add(GameAction.TurnRight);
            _model.Turn = 1;

            _model.TurnEnded();
            Assert.IsTrue(player1Turned);
            Assert.IsTrue(player2Turned);
            Assert.AreEqual(Orientation.Up, _model.Players[0].Orientation);
            Assert.AreEqual(Orientation.Up, _model.Players[1].Orientation);
        }
        [TestMethod]
        public async Task TestAttacks()
        {
            var stream = new MemoryStream();
            await _model.LoadGameAsync(stream);

            _model.Players[0].Actions.Add(GameAction.UseLaser);
            _model.Players[1].Actions.Add(GameAction.Melee);
            _model.Turn = 1;

            _model.TurnEnded();

            Assert.AreEqual(2, _model.Players[0].Health);
            Assert.AreEqual(2, _model.Players[1].Health);

            _model.Players[0].Actions.AddRange([GameAction.MoveLeft,GameAction.UseLaser]);
            _model.Players[1].Actions.AddRange([GameAction.MoveLeft,GameAction.Melee]);
            _model.Turn = 1;

            _model.TurnEnded();

            Assert.AreEqual(2, _model.Players[0].Health);
            Assert.AreEqual(2, _model.Players[1].Health);
        }
        [TestMethod]
        public async Task TestGameOver1Won()
        {
            var stream = new MemoryStream();
            await _model.LoadGameAsync(stream);

            bool player1Won = false;
            bool player2Won = false;
            bool tie = false;
            _model.GameOver += (sender, args) => { 
                if (args == GameEnd.Tie) { tie = true; }
                else if( args == GameEnd.Player1Won) { player1Won = true; }
                else if( args == GameEnd.Player2Won) { player2Won = true; }
            };
            _model.Turn = 1;
            _model.Players[0].Actions.AddRange([GameAction.UseLaser,GameAction.UseLaser,GameAction.UseLaser]);
            _model.Players[1].Actions.AddRange([GameAction.TurnLeft, GameAction.TurnLeft, GameAction.TurnLeft]);

            _model.TurnEnded();

            Assert.IsTrue(player1Won);
            Assert.IsFalse(player2Won);
            Assert.IsFalse(tie);

            Assert.AreEqual(0, _model.Players[1].Health);

            Assert.IsTrue(_model.IsGameOver);
        }
        [TestMethod]
        public async Task TestGameOverTie()
        {
            var stream = new MemoryStream();
            await _model.LoadGameAsync(stream);

            bool player1Won = false;
            bool player2Won = false;
            bool tie = false;
            _model.GameOver += (sender, args) => {
                if (args == GameEnd.Tie) { tie = true; }
                else if (args == GameEnd.Player1Won) { player1Won = true; }
                else if (args == GameEnd.Player2Won) { player2Won = true; }
            };
            _model.Turn = 1;
            _model.Players[0].Actions.AddRange([GameAction.UseLaser, GameAction.UseLaser, GameAction.UseLaser]);
            _model.Players[1].Actions.AddRange([GameAction.UseLaser, GameAction.UseLaser, GameAction.UseLaser]);

            _model.TurnEnded();

            Assert.IsTrue(tie);
            Assert.IsFalse(player2Won);
            Assert.IsFalse(player1Won);

            Assert.AreEqual(0, _model.Players[1].Health);
            Assert.AreEqual(0, _model.Players[0].Health);

            Assert.IsTrue(_model.IsGameOver);
        }

    }
}