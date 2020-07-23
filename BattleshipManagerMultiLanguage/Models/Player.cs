using System;
using System.Collections.Generic;

namespace BattleshipManagerMultiLanguage.Models
{
    public class Player
    {
        #region Private

        private readonly int _fieldHeight = 10;
        private readonly int _fieldWidth = 10;

        private readonly int _shipAmount;

        private string generateToken( )
        {
            return Convert.ToBase64String( Guid.NewGuid( ).ToByteArray( ) );
        }

        private bool isShipSettedRight( Ship ship )
        {
            if ( ship.Dir == Direction.HORIZONTAL )
            {
                for ( int i = 0; i < ship.Size; i++ )
                {
                    if ( this.ShipField[ship.X + i, ship.Y] )
                        return false;
                    if ( this.ShipField[ship.X + i + 1, ship.Y] )
                        return false;
                }

                for ( int i = 0; i < ship.Size; i++ )
                {
                    this.ShipField[ship.X + i, ship.Y] = true;
                }
            }
            else
            {
                for ( int i = 0; i < ship.Size - 1; i++ )
                {
                    if ( this.ShipField[ship.X, ship.Y + i] )
                        return false;
                    if ( this.ShipField[ship.X, ship.Y + i + 1] )
                        return false;
                }

                for ( int i = 0; i < ship.Size - 1; i++ )
                {
                    this.ShipField[ship.X, ship.Y + i] = true;
                }
            }

            return true;
        }

        #endregion

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

        public int winCount = 0;
        public bool winner = false;

        public Player( string name, bool autoSetShips = false, int shipAmount = 5 )
        {
            this.Name = name;
            this.Token = this.generateToken( );
            this._shipAmount = shipAmount;
            this.AutoSetShips = autoSetShips;
            this.Ships = autoSetShips ? this.SetShips( this.createShips( ) ) : this.createShips( );
        }

        /// <summary>   Creates the ships. </summary>
        /// <returns>   The new ships. </returns>
        public List<Ship> createShips( )
        {
            Random r = new Random( );
            List<Ship> ships = new List<Ship>( );
            for ( int i = 0; i < this._shipAmount; i++ )
            {
                ships.Add( new Ship( r.Next( 6 ) + 2 ) { X = 0, Y = 0 } );
            }

            return ships;
        }

        /// <summary>   Gets random direction. </summary>
        /// <returns>   The random direction. </returns>
        public Direction GetRandomDirection( )
        {
            Random random = new Random( );

            if ( random.Next( 2 ) == 0 )
                return Direction.HORIZONTAL;
            return Direction.VERTICAL;
        }

        /// <summary>   Sets the ships. </summary>
        /// <param name="ships">    The ships.</param>
        /// <returns>   A List&lt;Ship&gt; </returns>
        public List<Ship> SetShips( List<Ship> ships )
        {
            Random random = new Random( );

            foreach ( Ship ship in ships )
            {
                ship.Dir = this.GetRandomDirection( );

                if ( ship.Dir == Direction.HORIZONTAL )
                {
                    while ( !this.isShipSettedRight( ship ) )
                    {
                        ship.X = random.Next( 0, this._fieldWidth - ship.Size - 1 );
                        ship.Y = random.Next( 0, this._fieldHeight - 1 );
                    }
                }
                else
                {
                    while ( !this.isShipSettedRight( ship ) )
                    {
                        ship.X = random.Next( 0, this._fieldWidth - 1 );
                        ship.Y = random.Next( 0, this._fieldHeight - ship.Size - 1 );
                    }
                }
            }

            return ships;
        }

        #endregion
    }
}