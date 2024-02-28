using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt {
    public class GameBoard {

        #region --- Definitions ---
        public enum TILE : uint {
            EMPTY,
            TREASURE,
            TRAP,
            BLOCKED,
        }
        #endregion

        #region --- Constructor ---
        public GameBoard(int sizeX, int sizeY) {

            TILE[] rTiles = new TILE[] { TILE.EMPTY, TILE.BLOCKED, TILE.TRAP, TILE.TREASURE };
            SizeY = sizeY;
            SizeX = sizeX;
            Tiles = new TILE[sizeX * sizeY];
            for(int i = 0; i < sizeX*sizeY; i++) {
                Tiles[i] = TILE.EMPTY;
            }
        }
        #endregion

        #region --- Public Properties ----
        public int SizeY { get; private set; }
        public int SizeX { get; private set; }
        #endregion

        #region --- Public Access Functions ---
        public TILE Field(int x, int y) {
            if ((y < 0) || (y >= SizeY)) return TILE.BLOCKED;
            if ((x < 0) || (x >= SizeX)) return TILE.BLOCKED;
            return Tiles[x * SizeY + y];
        }

        public void SetField(int x, int y, TILE t) {

            if ((y < 0) || (y >= SizeY)) return ;
            if ((x < 0) || (x >= SizeX)) return;
            Tiles[x * SizeY + y] = t;
        }

        #endregion

        #region --- Private Members ---

        private TILE[] Tiles;
        
        #endregion
    }
}
