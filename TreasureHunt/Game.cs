using AI;
using AI.QLearning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TreasureHunt {

    // the game class implements the IGameState interface required for QLearning
    class Game : IGameState {

        #region --- Constructor ---
        public Game(string[] fields) {

            int numRows = fields.Length;
            int numCols = fields[0].Length;

            PlayerPosX = 0;
            PlayerPosY = 0;

            Board = new GameBoard(numCols, numRows);

            for (int y = 0; y < numRows; y++) {
                string row = fields[y];
                if (row.Length != numCols) {
                    throw new ArgumentException("Incosistent field definition");
                }
                for (int x = 0; x < row.Length; x++) {
                    char c = row[x];
                    switch(c) {
                        default:
                        case ' ':
                            Board.SetField(x, y, GameBoard.TILE.EMPTY);
                            break;
                        case 'X':
                            Board.SetField(x, y, GameBoard.TILE.BLOCKED);
                            break;
                        case 'G':
                            Board.SetField(x, y, GameBoard.TILE.TREASURE);
                            break;
                        case 'T':
                            Board.SetField(x, y, GameBoard.TILE.TRAP);
                            break;
                        case 'S':
                            StartX = x;
                            StartY = y;
                            Board.SetField(x, y, GameBoard.TILE.EMPTY);
                            break;
                    }
                }
            }

            AllActions = new Action[(int) Motion.NumActions];
            AllActions[(int) Motion.Up    ] = new Action { Name ="Move Up",    Movement = Motion.Up      };
            AllActions[(int) Motion.Down  ] = new Action { Name ="Move Down",  Movement = Motion.Down    };
            AllActions[(int) Motion.Left  ] = new Action { Name ="Move Left",  Movement = Motion.Left    };
            AllActions[(int) Motion.Right ] = new Action { Name ="Move Right", Movement = Motion.Right   };

            Reset();
        }
        #endregion

        #region --- Public Properties ---
        // current game board
        public GameBoard Board { get; private set; }

        // current tileposition x,y of the player
        public int PlayerPosX { get; private set; }
        public int PlayerPosY { get; private set; }

        // current health points (maximum is 3)
        public int PlayerHealth { get; private set; }

        // reward for picking up the treasure
        public double Reward { get; set; } = 1.0;

        // penalties if health decreses to x health points
        public double[] Penalties { get; } = new double[] { -1.0, -1.0, -1.0 };
        #endregion

        #region --- Public Member Functions ---
        public void Reset() {
            PlayerPosX = StartX;
            PlayerPosY = StartY;
            PlayerHealth = 3;
        }
        #endregion

        #region --- IGameState Interface Implementation ----
        public uint Id {
            get {
                uint result = 0;
                result |= (uint) (PlayerPosX & 0x1F);
                result |= (uint) (PlayerPosY & 0x1F) << 5;
                result |= (uint) (PlayerHealth & 0x03) << 10;
                return result;
            }
        }
        public List<IAction> PossibleActions {
            get {
                List<IAction> actions = new List<IAction>();
                if (Board.Field(PlayerPosX, PlayerPosY+1) != GameBoard.TILE.BLOCKED) {
                    actions.Add(AllActions[(int) Motion.Down]);
                }
                if (Board.Field(PlayerPosX, PlayerPosY -1) != GameBoard.TILE.BLOCKED) {
                    actions.Add(AllActions[(int)Motion.Up]);
                }
                if (Board.Field(PlayerPosX+1, PlayerPosY) != GameBoard.TILE.BLOCKED) {
                    actions.Add(AllActions[(int)Motion.Right]);
                }
                if (Board.Field(PlayerPosX-1, PlayerPosY) != GameBoard.TILE.BLOCKED) {
                    actions.Add(AllActions[(int)Motion.Left]);
                }
                return actions;
            }
        }
        public double ExecuteAction(IAction a) {

            if (a is Action action) {
                switch (action.Movement) {
                    case Motion.Down:
                        return MovePlayer(PlayerPosX, PlayerPosY + 1);
                    case Motion.Up:
                        return MovePlayer(PlayerPosX, PlayerPosY - 1);
                    case Motion.Left:
                        return MovePlayer(PlayerPosX - 1, PlayerPosY);
                    case Motion.Right:
                        return MovePlayer(PlayerPosX + 1, PlayerPosY);
                }
            }

            throw new ArgumentException("Not a valid action!");
        }
        #endregion

        #region --- Public Execute Function For Manual Playing
        public double TryExecuteAction(string actionName) {
            var a = (from x in PossibleActions
                     where (x.Name == actionName)
                     select x).FirstOrDefault();
            if (a != null) {
                return ExecuteAction(a);
            }
            return 0.0;
        }
        #endregion

        #region --- Private Helper Functions ---
        private double MovePlayer(int x, int y) {

            if (x < 0) throw new ArgumentException("Invalid Move: x < 0");
            if (y < 0) throw new ArgumentException("Invalid Move: y < 0");
            if (x >= Board.SizeX) throw new ArgumentException("Invalid Move: x >= " + Board.SizeX);
            if (y >= Board.SizeY) throw new ArgumentException("Invalid Move: y >= " + Board.SizeY);

            if (Board.Field(x, y) == GameBoard.TILE.BLOCKED) {
                throw new ArgumentException("Invalid Move: Tile(" + x + "," + y + ") is blocked!");
            }

            PlayerPosX = x;
            PlayerPosY = y;

            double result = 0.0;
            switch (Board.Field(x, y)) {
                case GameBoard.TILE.TRAP:
                    PlayerHealth--;
                    result = Penalties[PlayerHealth];
                    if (PlayerHealth == 0) Reset();
                    break;
                case GameBoard.TILE.TREASURE:
                    Reset();
                    result = Reward;
                    break;
                default:
                    break;
            }
            return result;

        }
        #endregion

        #region --- Private Definition of the Actions ---
        enum Motion : int {
            Up = 0,
            Down,
            Left,
            Right,
            NumActions
        }

        private class Action : IAction {
            public Action() { }
            public Motion Movement { get; set; }
            public string Name { get; set; }
            public override string ToString() {
                return Name;
            }
        }
        #endregion

        #region --- Private Members ---
        private int StartX, StartY;
        private Action[] AllActions;
        #endregion
    }
}
