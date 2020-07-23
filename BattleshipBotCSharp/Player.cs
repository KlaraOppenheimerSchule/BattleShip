using System;
using System.Collections.Generic;

namespace BattleshipBotCSharp
{
    public class Player
    {
        #region Public

        public bool activePlayer = false;

        public bool AutoSetShips;
        public bool gameEnded = false;
        public bool looser = false;

        public string Name;
        public Boolean[,] ShipField = new bool[10, 10];
        public List<Ship> Ships = new List<Ship>( );
        public Boolean[,] ShotField = new bool[10, 10];
        public string Token;
        public bool winner = false;

        public Player( string name, bool autoSetShips = false, int shipAmount = 5 )
        {
            this.Name = name;
            this.AutoSetShips = autoSetShips;
        }

        #endregion
    }
}